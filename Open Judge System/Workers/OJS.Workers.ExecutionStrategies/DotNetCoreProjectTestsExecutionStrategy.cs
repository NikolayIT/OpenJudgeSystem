namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Compilers;
    using OJS.Workers.Executors;

    public class DotNetCoreProjectTestsExecutionStrategy : CSharpProjectTestsExecutionStrategy
    {
        private new const string AdditionalExecutionArguments = "--noresult";
        private const string AdditionalCompilerArguments =
            "/p:Configuration=Release,Optimize=true /verbosity:quiet /nologo";

        private const string CsProjFileExtention = ".csproj";
        private const string NunitLiteConsoleAppFolderName = "NunitLiteConsoleApp";
        private const string ProjectPathPlaceholder = "##projectPath##";
        private const string NunitLiteConsoleAppProgramTemplate = @"using System;
            using System.Reflection;
            using NUnit.Common;
            using NUnitLite;
            
            public class Program
            {
                public static void Main(string[] args)
                {
                    var writter = new ExtendedTextWrapper(Console.Out);
                    new AutoRun(typeof(Program).GetTypeInfo().Assembly).Execute(args, writter, Console.In);
                }
            }";

        private readonly string nunitLiteConsoleAppCsProjTemplate =
            $@"<Project Sdk=""Microsoft.NET.Sdk"">
                <PropertyGroup>
                    <OutputType>Exe</OutputType>
                    <TargetFramework>netcoreapp2.0</TargetFramework>
                </PropertyGroup>

                <ItemGroup>
                    <PackageReference Include=""NUnitLite"" Version=""3.8.1"" />
                </ItemGroup>

                <ItemGroup>
                    <ProjectReference Include=""{ProjectPathPlaceholder}"" />
                </ItemGroup>
            </Project>";

        private readonly string consoleAppEntryPointClassFullName =
            $"Program{GlobalConstants.CSharpFileExtension}";

        private readonly string consoleAppEntryPointCsProjFullName =
            $"{NunitLiteConsoleAppFolderName}{CsProjFileExtention}";

        public DotNetCoreProjectTestsExecutionStrategy(Func<CompilerType, string> getCompilerPathFunc) 
            : base(getCompilerPathFunc)
        {
        }

        private string NunitLiteConsoleAppDirectory =>
            Path.Combine(this.WorkingDirectory, NunitLiteConsoleAppFolderName);

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            var userSubmission = executionContext.FileContent;

            this.ExtractFilesInWorkingDirectory(userSubmission);
            this.CreateNunitLiteConsoleAppDirectory();
            this.ExtractTestNames(executionContext.Tests);
            this.WriteTestFiles(executionContext.Tests, this.NunitLiteConsoleAppDirectory);

            var userCsProjFilePath = this.GetCsProjFilePath();

            var compilerPath = this.GetCompilerPathFunc(executionContext.CompilerType);
            var compilerResult = this.Compile(
                executionContext.CompilerType,
                compilerPath,
                AdditionalCompilerArguments,
                userCsProjFilePath);

            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
            
            if (!compilerResult.IsCompiledSuccessfully)
            {
                result.CompilerComment = compilerResult.CompilerComment;
                return result;
            }

            var nunitLiteConsoleAppCsProjPath = this.CreateNunitLiteConsoleApp(
                NunitLiteConsoleAppProgramTemplate,
                this.nunitLiteConsoleAppCsProjTemplate,
                this.NunitLiteConsoleAppDirectory,
                userCsProjFilePath);

            var consoleAppCompilerResult = this.Compile(
                executionContext.CompilerType,
                compilerPath,
                AdditionalCompilerArguments,
                nunitLiteConsoleAppCsProjPath);

            // Delete tests before execution so the user can't access them
            FileHelpers.DeleteFiles(this.TestPaths.ToArray());

            var executor = new RestrictedProcessExecutor();
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            result = this.RunNunitLiteTests(
                compilerPath,
                executionContext,
                executor,
                checker,
                result,
                consoleAppCompilerResult.OutputFile);

            return result;
        }

        protected override CompileResult Compile(
            CompilerType compilerType,
            string compilerPath,
            string compilerArguments,
            string submissionFilePath)
        {
            if (compilerType == CompilerType.None)
            {
                return new CompileResult(true, null) { OutputFile = submissionFilePath };
            }

            if (!File.Exists(compilerPath))
            {
                throw new ArgumentException($@"Compiler not found in: {compilerPath}", nameof(compilerPath));
            }

            var compiler = Compiler.CreateCompiler(compilerType);
            var compilerResult = compiler.Compile(compilerPath, submissionFilePath, compilerArguments);

            return compilerResult;
        }

        private ExecutionResult RunNunitLiteTests(
            string compilerPath,
            ExecutionContext executionContext,
            IExecutor executor,
            IChecker checker,
            ExecutionResult result,
            string compiledFile)
        {
            var arguments = new List<string> { compiledFile };
            arguments.AddRange(AdditionalExecutionArguments.Split(' '));

            var processExecutionResult = executor.Execute(
                compilerPath,
                string.Empty,
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                arguments,
                this.WorkingDirectory);

            var (totalTestsCount, failedTestsCount) = this.ExtractTotalFailedTestsCount(processExecutionResult.ReceivedOutput);
            var errorsByFiles = this.GetTestErrors(processExecutionResult.ReceivedOutput);

            if (failedTestsCount != errorsByFiles.Count || totalTestsCount != executionContext.Tests.Count())
            {
                throw new ArgumentException("Failing tests not captured properly, please contact an administrator");
            }

            var testIndex = 0;

            foreach (var test in executionContext.Tests)
            {
                var message = "Test Passed!";
                var testFile = this.TestNames[testIndex++];
                if (errorsByFiles.ContainsKey(testFile))
                {
                    message = errorsByFiles[testFile];
                }

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, message);
                result.TestResults.Add(testResult);
            }

            return result;
        }

        private string CreateNunitLiteConsoleApp(
            string nUnitLiteProgramTemplate,
            string nUnitLiteCsProjTemplate,
            string directoryPath,
            string csProjToTestFilePath)
        {
            var consoleAppEntryPointPath = $@"{directoryPath}\{this.consoleAppEntryPointClassFullName}";
            File.WriteAllText(consoleAppEntryPointPath, nUnitLiteProgramTemplate);

            nUnitLiteCsProjTemplate = nUnitLiteCsProjTemplate
                .Replace(ProjectPathPlaceholder, csProjToTestFilePath);

            var consoleAppCsProjPath = $@"{directoryPath}\{this.consoleAppEntryPointCsProjFullName}";
            File.WriteAllText(consoleAppCsProjPath, nUnitLiteCsProjTemplate);

            return consoleAppCsProjPath;
        }

        private void CreateNunitLiteConsoleAppDirectory()
        {
            if (!Directory.Exists(this.NunitLiteConsoleAppDirectory))
            {
                Directory.CreateDirectory(this.NunitLiteConsoleAppDirectory);
            }
        }
    }
}