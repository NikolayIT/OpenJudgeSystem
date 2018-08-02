namespace OJS.Workers.ExecutionStrategies.BlockchainStrategies
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Common;
    using OJS.Workers.ExecutionStrategies;
    using OJS.Workers.Executors;

    public class SolidityCompileDeployAndRunUnitTestsExecutionStrategy : ExecutionStrategy
    {
        private const string AbiFileSearchPattern = "*.abi";
        private const string ContractNameRegexPattern = @"^\s*contract\s+([A-Za-z]\w*)\s*\{";
        private const string TestsCountRegexPattern = @"^\s*(\d+)\s{1}passing\s\(\d+\w+\)\s*((\d+)\sfailing\s*$)*";
        private readonly string nodeJsExecutablePath;
        private readonly string ganacheNodeCliPath;
        private readonly string truffleExecutablePath;
        private readonly int portNumber;

        public SolidityCompileDeployAndRunUnitTestsExecutionStrategy(
            Func<CompilerType, string> getCompilerPathFunc,
            string nodeJsExecutablePath,
            string ganacheNodeCliPath,
            string truffleExecutablePath,
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

            if (!File.Exists(ganacheNodeCliPath))
            {
                throw new ArgumentException(
                    $"Ganache-cli not found in: {ganacheNodeCliPath}",
                    nameof(ganacheNodeCliPath));
            }

            if (!File.Exists(truffleExecutablePath))
            {
                throw new ArgumentException(
                    $"Truffle not found in: {truffleExecutablePath}",
                    nameof(truffleExecutablePath));
            }

            this.nodeJsExecutablePath = nodeJsExecutablePath;
            this.ganacheNodeCliPath = ganacheNodeCliPath;
            this.truffleExecutablePath = truffleExecutablePath;
            this.portNumber = portNumber;
            this.GetCompilerPathFunc = getCompilerPathFunc;
        }

        protected Func<CompilerType, string> GetCompilerPathFunc { get; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            var contractName = Regex
                .Match(executionContext.Code, ContractNameRegexPattern, RegexOptions.Multiline)
                ?.Groups[1]
                ?.Value;

            if (string.IsNullOrEmpty(contractName))
            {
                throw new ArgumentException("No valid contract is found");
            }

            // Compile the file
            var compilerResult = this.ExecuteCompiling(executionContext, this.GetCompilerPathFunc, result);
            if (!compilerResult.IsCompiledSuccessfully)
            {
                return result;
            }

            var(byteCode, abi) = GetByteCodeAndAbi(compilerResult.OutputFile);

            var truffleProject = new TruffleProjectManager(this.WorkingDirectory, this.portNumber);

            truffleProject.InitializeMigration(this.GetCompilerPathFunc(executionContext.CompilerType));
            truffleProject.CreateBuildForContract(contractName, abi, byteCode);
            truffleProject.ImportJsUnitTests(executionContext.Tests);

            IExecutor executor = new StandardProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);
            ProcessExecutionResult processExecutionResult;

            // Run in the Ethereum Virtual Machine scope
            using (new GanacheCliScope(this.nodeJsExecutablePath, this.ganacheNodeCliPath, this.portNumber))
            {
                // Execute tests
                processExecutionResult = executor.Execute(
                    this.truffleExecutablePath,
                    string.Empty,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    new[] { "test" },
                    this.WorkingDirectory);
            }

            if (!string.IsNullOrWhiteSpace(processExecutionResult.ErrorOutput))
            {
                throw new ArgumentException(processExecutionResult.ErrorOutput);
            }

            processExecutionResult.RemoveColorEncodingsFromReceivedOutput();

            var(totalTestsCount, failingTestsCount) =
                ExtractFailingTestsCount(processExecutionResult.ReceivedOutput);

            throw new NotImplementedException();
        }

        private static(string byteCode, string abi) GetByteCodeAndAbi(string compilerResultOutputFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(compilerResultOutputFile);

            var byteCode = File.ReadAllText(compilerResultOutputFile);

            var abiFile = FileHelpers
                .FindAllFilesMatchingPattern(Path.GetDirectoryName(compilerResultOutputFile), AbiFileSearchPattern)
                .First(f => Path.GetFileNameWithoutExtension(f) == fileName);

            var abi = File.ReadAllText(abiFile);

            return (byteCode, abi);
        }

        private static(int totalTestsCount, int failingTestsCount) ExtractFailingTestsCount(string receivedOutput)
        {
            int totalTestsCount;
            int failingTestsCount;

            var match = Regex.Match(receivedOutput, TestsCountRegexPattern, RegexOptions.Multiline);

            if (match.Success)
            {
                var passingTests = int.Parse(match.Groups[1].Value);
                int.TryParse(match.Groups[3]?.Value, out failingTestsCount);

                totalTestsCount = passingTests + failingTestsCount;
            }
            else
            {
                throw new ArgumentException("The process did not produce any output!");
            }

            return (totalTestsCount, failingTestsCount);
        }
    }
}