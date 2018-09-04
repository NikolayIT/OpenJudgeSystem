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
    using OJS.Workers.Executors;

    public class DotNetCoreProjectTestsExecutionStrategy : CSharpProjectTestsExecutionStrategy
    {
        protected new const string AdditionalExecutionArguments = "--noresult";
        protected const string CsProjFileExtention = ".csproj";

        private const string ProjectPathPlaceholder = "##projectPath##";
        private const string ProjectReferencesPlaceholder = "##ProjectReferences##";
        private const string NUnitLiteConsoleAppFolderName = "NUnitLiteConsoleApp";
        private const string UserSubmissionFolderName = "UserProject";
        private const string NUnitLiteConsoleAppProgramName = "Program";
        private const string NUnitLiteConsoleAppProgramTemplate = @"
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
                    <PackageReference Include=""NUnitLite"" Version=""3.10.1"" />
                    <PackageReference Include=""Microsoft.EntityFrameworkCore.InMemory"" Version=""2.1.1"" />
                    <PackageReference Include=""Microsoft.EntityFrameworkCore.Proxies"" Version=""2.1.1"" />
                </ItemGroup>
                <ItemGroup>
                    {ProjectReferencesPlaceholder}
                </ItemGroup>
            </Project>";

        private readonly string projectReferenceTemplate =
            $@"<ProjectReference Include=""{ProjectPathPlaceholder}"" />";

        public DotNetCoreProjectTestsExecutionStrategy(
            Func<CompilerType, string> getCompilerPathFunc,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(getCompilerPathFunc, baseTimeUsed, baseMemoryUsed)
        {
        }

        protected string NUnitLiteConsoleAppDirectory =>
            Path.Combine(this.WorkingDirectory, NUnitLiteConsoleAppFolderName);

        protected string UserProjectDirectory =>
            Path.Combine(this.WorkingDirectory, UserSubmissionFolderName);

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            Directory.CreateDirectory(this.NUnitLiteConsoleAppDirectory);
            Directory.CreateDirectory(this.UserProjectDirectory);

            var result = new ExecutionResult();

            var userSubmission = executionContext.FileContent;

            this.ExtractFilesInWorkingDirectory(userSubmission, this.UserProjectDirectory);
            this.ExtractTestNames(executionContext.Tests);

            this.SaveTestFiles(executionContext.Tests, this.NUnitLiteConsoleAppDirectory);
            this.SaveSetupFixture(this.NUnitLiteConsoleAppDirectory);

            var userCsProjPaths = FileHelpers.FindAllFilesMatchingPattern(
                this.UserProjectDirectory, CsProjFileSearchPattern);

            var nunitLiteConsoleApp = this.CreateNunitLiteConsoleApp(userCsProjPaths);

            var compilerPath = this.GetCompilerPathFunc(executionContext.CompilerType);

            var compilerResult = this.Compile(
                executionContext.CompilerType,
                compilerPath,
                executionContext.AdditionalCompilerArguments,
                nunitLiteConsoleApp.csProjPath);

            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;

            if (!result.IsCompiledSuccessfully)
            {
                result.CompilerComment = compilerResult.CompilerComment;
                return result;
            }

            // Delete tests before execution so the user can't access them
            FileHelpers.DeleteFiles(this.TestPaths.ToArray());

            var executor = new RestrictedProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);
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
                compilerResult.OutputFile,
                AdditionalExecutionArguments);

            return result;
        }

        protected (string csProjTemplate, string csProjPath) CreateNunitLiteConsoleApp(
            IEnumerable<string> projectsToTestCsProjPaths)
        {
            var consoleAppEntryPointPath =
                $@"{this.NUnitLiteConsoleAppDirectory}\{NUnitLiteConsoleAppProgramName}{GlobalConstants.CSharpFileExtension}";

            File.WriteAllText(consoleAppEntryPointPath, NUnitLiteConsoleAppProgramTemplate);

            var references = projectsToTestCsProjPaths
                .Select(path => this.projectReferenceTemplate.Replace(ProjectPathPlaceholder, path));

            var csProjTemplate = this.nunitLiteConsoleAppCsProjTemplate
                .Replace(ProjectReferencesPlaceholder, string.Join(Environment.NewLine, references));

            var csProjPath = this.CreateNuinitLiteConsoleAppCsProjFile(csProjTemplate);

            return (csProjTemplate, csProjPath);
        }

        protected string CreateNuinitLiteConsoleAppCsProjFile(string csProjTemplate)
        {
            var consoleAppCsProjPath =
                $@"{this.NUnitLiteConsoleAppDirectory}\{NUnitLiteConsoleAppFolderName}{CsProjFileExtention}";

            File.WriteAllText(consoleAppCsProjPath, csProjTemplate);

            return consoleAppCsProjPath;
        }
    }
}