namespace OJS.Workers.ExecutionStrategies.BlockchainStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.ExecutionStrategies;
    using OJS.Workers.Executors;

    public class SolidityCompileDeployAndRunUnitTestsExecutionStrategy : ExecutionStrategy
    {
        private const string TestsCountRegexPattern = @"^\s*(\d+)\s{1}passing\s\(\d+\w+\)\s*((\d+)\sfailing\s*$)*";
        private const string TestNamesSearchPattern = @"it\((""|')(.+)(?:\1)(?=\s*,)";
        private const string FailingTestsSearchPattern = @"^[^\r?\n]\s*\d+\)\sContract:\s[^\s]+\r?\n";

        private readonly string nodeJsExecutablePath;
        private readonly string ganacheCliNodeExecutablePath;
        private readonly string truffleCliNodeExecutablePath;
        private readonly int portNumber;

        public SolidityCompileDeployAndRunUnitTestsExecutionStrategy(
            Func<CompilerType, string> getCompilerPathFunc,
            string nodeJsExecutablePath,
            string ganacheCliNodeExecutablePath,
            string truffleCliNodeExecutablePath,
            int portNumber,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(baseTimeUsed, baseMemoryUsed)
        {
            if (!File.Exists(nodeJsExecutablePath))
            {
                throw new ArgumentException(
                    $"NodeJS not found in: {nodeJsExecutablePath}",
                    nameof(nodeJsExecutablePath));
            }

            if (!File.Exists(ganacheCliNodeExecutablePath))
            {
                throw new ArgumentException(
                    $"Ganache-cli not found in: {ganacheCliNodeExecutablePath}",
                    nameof(ganacheCliNodeExecutablePath));
            }

            if (!File.Exists(truffleCliNodeExecutablePath))
            {
                throw new ArgumentException(
                    $"Truffle not found in: {truffleCliNodeExecutablePath}",
                    nameof(truffleCliNodeExecutablePath));
            }

            this.nodeJsExecutablePath = nodeJsExecutablePath;
            this.ganacheCliNodeExecutablePath = ganacheCliNodeExecutablePath;
            this.truffleCliNodeExecutablePath = truffleCliNodeExecutablePath;
            this.portNumber = portNumber;
            this.GetCompilerPathFunc = getCompilerPathFunc;
        }

        protected Func<CompilerType, string> GetCompilerPathFunc { get; }

        private static string TestFileNameSearchPattern =>
            $@"{TruffleProjectManager.TestFileNamePrefix}(\d+){GlobalConstants.JavaScriptFileExtension}";

        private IList<string> TestNames { get; } = new List<string>();

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            this.ExtractTestNames(executionContext.Tests);

            // Compile the file
            var compilerResult = this.ExecuteCompiling(executionContext, this.GetCompilerPathFunc, result);
            if (!compilerResult.IsCompiledSuccessfully)
            {
                return result;
            }

            var compiledContracts = GetCompiledContracts(Path.GetDirectoryName(compilerResult.OutputFile));

            var truffleProject = new TruffleProjectManager(this.WorkingDirectory, this.portNumber);

            truffleProject.InitializeMigration(this.GetCompilerPathFunc(executionContext.CompilerType));
            truffleProject.CreateJsonBuildForContracts(compiledContracts);
            truffleProject.ImportJsUnitTests(executionContext.Tests);

            IExecutor executor = new StandardProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);
            ProcessExecutionResult processExecutionResult;

            // Run tests in the Ethereum Virtual Machine scope
            using (var ganache = new GanacheCli(this.nodeJsExecutablePath, this.ganacheCliNodeExecutablePath))
            {
                ganache.Listen(this.portNumber);

                processExecutionResult = executor.Execute(
                    this.nodeJsExecutablePath,
                    string.Empty,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    new[] { $"{this.truffleCliNodeExecutablePath} test" },
                    this.WorkingDirectory);
            }

            if (!string.IsNullOrWhiteSpace(processExecutionResult.ErrorOutput))
            {
                throw new ArgumentException(processExecutionResult.ErrorOutput);
            }

            processExecutionResult.RemoveColorEncodingsFromReceivedOutput();

            var (totalTestsCount, failingTestsCount) =
                ExtractFailingTestsCount(processExecutionResult.ReceivedOutput);

            if (totalTestsCount != executionContext.Tests.Count())
            {
                throw new ArgumentException(
                    "Some of the tests contain more than one test per test case. Please contact an administrator");
            }

            var errorsByTestNames = this.GetErrorsByTestNames(processExecutionResult.ReceivedOutput);

            if (errorsByTestNames.Count != failingTestsCount)
            {
                throw new ArgumentException("Failing tests not captured properly. Please contact an administrator");
            }

            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            var testsCounter = 0;
            foreach (var test in executionContext.Tests)
            {
                var message = "Test Passed!";
                var testName = this.TestNames[testsCounter++];
                if (errorsByTestNames.ContainsKey(testName))
                {
                    message = errorsByTestNames[testName];
                }

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, message);
                result.TestResults.Add(testResult);
            }

            return result;
        }

        private static Dictionary<string, (string byteCode, string abi)> GetCompiledContracts(
            string compilationDirectoryPath)
        {
            var contracts = new Dictionary<string, (string byteCode, string abi)>();

            var compilationDirectoryInfo = new DirectoryInfo(compilationDirectoryPath);

            foreach (var file in compilationDirectoryInfo.GetFiles())
            {
                var contractname = Path.GetFileNameWithoutExtension(file.FullName);
                var fileContent = File.ReadAllText(file.FullName);

                string byteCode = null;
                string abi = null;

                if (file.Extension == GlobalConstants.ByteCodeFileExtension)
                {
                    byteCode = fileContent;
                }
                else if (file.Extension == GlobalConstants.AbiFileExtension)
                {
                    abi = fileContent;
                }

                if (!contracts.ContainsKey(contractname))
                {
                    contracts.Add(contractname, (byteCode, abi));
                }
                else
                {
                    byteCode = contracts[contractname].byteCode ?? byteCode;
                    abi = contracts[contractname].abi ?? abi;

                    contracts[contractname] = (byteCode, abi);
                }
            }

            return contracts;
        }

        private static (int totalTestsCount, int failingTestsCount) ExtractFailingTestsCount(string receivedOutput)
        {
            int totalTestsCount;
            int failingTestsCount;

            var match = Regex.Match(receivedOutput, TestsCountRegexPattern, RegexOptions.Multiline);

            if (match.Success)
            {
                var passingTests = int.Parse(match.Groups[1].Value);
                int.TryParse(match.Groups[3].Value, out failingTestsCount);

                totalTestsCount = passingTests + failingTestsCount;
            }
            else if (int.TryParse(
                Regex.Match(receivedOutput, TestFileNameSearchPattern).Groups[1].Value,
                out var invalidTestNumber))
            {
                throw new ArgumentException(
                    $"Test {invalidTestNumber} might be invalid. Please contact an administrator.");
            }
            else
            {
                throw new ArgumentException("The process did not produce any output!");
            }

            return (totalTestsCount, failingTestsCount);
        }

        private Dictionary<string, string> GetErrorsByTestNames(string receivedOutput)
        {
            var errorsByTestNames = new Dictionary<string, string>();

            var errorMessages = Regex
                .Split(receivedOutput, FailingTestsSearchPattern, RegexOptions.Multiline)
                .Skip(1);

            foreach (var errorMessage in errorMessages)
            {
                foreach (var testName in this.TestNames)
                {
                    if (errorMessage.Contains(testName + ":") ||
                        errorMessage.Contains(testName + "\":"))
                    {
                        var message = errorMessage
                            .Substring(errorMessage.IndexOf(testName, StringComparison.Ordinal))
                            .RemoveMultipleSpaces()
                            .ToSingleLine();

                        errorsByTestNames.Add(testName, message);
                        break;
                    }
                }
            }

            return errorsByTestNames;
        }

        private void ExtractTestNames(IEnumerable<TestContext> tests)
        {
            var testNum = 1;
            foreach (var test in tests)
            {
                var testNameMatch = Regex.Match(test.Input, TestNamesSearchPattern);
                var testName = testNameMatch.Groups[2].Value;

                if (!testNameMatch.Success || string.IsNullOrWhiteSpace(testName))
                {
                    throw new ArgumentException($"Test {testNum} (Id: {test.Id}) is invalid.");
                }

                if (this.TestNames.Contains(testName))
                {
                    throw new ArgumentException($"Two tests with the same name found. Name of Test {testNum}");
                }

                this.TestNames.Add(testName);
                testNum++;
            }
        }
    }
}