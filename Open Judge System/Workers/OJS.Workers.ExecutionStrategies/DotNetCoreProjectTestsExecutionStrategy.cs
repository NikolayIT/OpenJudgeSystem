namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Executors;

    public class DotNetCoreProjectTestsExecutionStrategy : CSharpProjectTestsExecutionStrategy
    {
        private new const string AdditionalExecutionArguments = "--noresult";
        private const string CsProjFileExtention = ".csproj";
        private const string NunitLiteConsoleAppFolderName = "NunitLiteConsoleApp";
        private const string ProjectPathPlaceholder = "##projectPath##";
        private const string NunitLiteConsoleAppProgramTemplate = @"
            using System;
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

        private readonly string nunitLiteConsoleAppCsProjTemplate = $@"
            <Project Sdk=""Microsoft.NET.Sdk"">
                <PropertyGroup>
                    <OutputType>Exe</OutputType>
                    <TargetFramework>netcoreapp2.0</TargetFramework>
                </PropertyGroup>
                <ItemGroup>
                    <PackageReference Include=""NUnitLite"" Version=""3.8.1"" />
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

            var nunitLiteConsoleAppCsProjPath = this.CreateNunitLiteConsoleApp(
                NunitLiteConsoleAppProgramTemplate,
                this.nunitLiteConsoleAppCsProjTemplate,
                this.NunitLiteConsoleAppDirectory,
                userCsProjFilePath);

            var compilerPath = this.GetCompilerPathFunc(executionContext.CompilerType);

            var consoleAppCompilerResult = this.Compile(
                executionContext.CompilerType,
                compilerPath,
                executionContext.AdditionalCompilerArguments,
                nunitLiteConsoleAppCsProjPath);

            result.IsCompiledSuccessfully = consoleAppCompilerResult.IsCompiledSuccessfully;
            
            if (!consoleAppCompilerResult.IsCompiledSuccessfully)
            {
                result.CompilerComment = consoleAppCompilerResult.CompilerComment;
                return result;
            }

            // Delete tests before execution so the user can't access them
            FileHelpers.DeleteFiles(this.TestPaths.ToArray());

            var executor = new RestrictedProcessExecutor();
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            result = this.RunUnitTests(
                compilerPath,
                executionContext,
                executor,
                checker,
                result,
                consoleAppCompilerResult.OutputFile,
                AdditionalExecutionArguments);

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