namespace OJS.Workers.ExecutionStrategies
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions; 
      
    using Microsoft.Build.Evaluation;

    using OJS.Common.Extensions;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Executors;
   
    public class CSharpUnitTestsRunnerExecutionStrategy : ExecutionStrategy
    {
        private const string ZippedSubmissionName = "Submission.zip";
        private const string TestedCode = "TestedCode.cs";
        private const string CsProjFileSearchPattern = "*.csproj";
        private const string ProjectFileExtenstion = ".csproj";
        private const string DllFileExtension = ".dll";
        private const string CompiledResultFolder = "compiledResult\\";
        private const string NUnitReference =
            "nunit.framework, Version=3.6.0.0, Culture=neutral, PublicKeyToken=2638cd05610744eb, processorArchitecture=MSIL";

        // Extracts the number of total and passed tests 
        private const string TestResultsRegex =
            @"Test Count: (\d+), Passed: (\d+), Failed: (\d+), Warnings: \d+, Inconclusive: \d+, Skipped: \d+";

        public CSharpUnitTestsRunnerExecutionStrategy(string nUnitConsoleRunnerPath)
        {
            this.NUnitConsoleRunnerPath = nUnitConsoleRunnerPath;
            this.WorkingDirectory = DirectoryHelpers.CreateTempDirectory();
        }

        ~CSharpUnitTestsRunnerExecutionStrategy()
        {
            DirectoryHelpers.SafeDeleteDirectory(this.WorkingDirectory, true);
        }

        protected string NUnitConsoleRunnerPath { get; set; }

        protected string WorkingDirectory { get; set; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();
            var userSubmissionContent = executionContext.FileContent;

            var submissionFilePath = $"{this.WorkingDirectory}\\{ZippedSubmissionName}";
            File.WriteAllBytes(submissionFilePath, userSubmissionContent);
            FileHelpers.UnzipFile(submissionFilePath, this.WorkingDirectory);
            File.Delete(submissionFilePath);

            var csProjFilePath = FileHelpers.FindFirstFileMatchingPattern(
                this.WorkingDirectory,
                CsProjFileSearchPattern);

            var project = new Project(csProjFilePath);
            this.CorrectProjectReferences(project);
            project.Save(csProjFilePath);
            project.ProjectCollection.UnloadAllProjects();

            result.IsCompiledSuccessfully = true;

            var executor = new RestrictedProcessExecutor();
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            result = this.RunUnitTests(executionContext, executor, checker, result, csProjFilePath);
            return result;
        }

        private ExecutionResult RunUnitTests(
            ExecutionContext executionContext, 
            IExecutor executor,
            IChecker checker,
            ExecutionResult result, 
            string csProjFilePath)
        {
            var compileDirectory = Path.GetDirectoryName(csProjFilePath);
            var fileName = Path.GetFileName(csProjFilePath);
            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = fileName.Replace(ProjectFileExtenstion, DllFileExtension);
            }

            var targetDll = $"{compileDirectory}\\{CompiledResultFolder}{fileName}";
            var arguments = new List<string> { targetDll };
            arguments.AddRange(executionContext.AdditionalCompilerArguments.Split(' '));

            var testedCodePath = $"{compileDirectory}\\{TestedCode}";
            var originalTestsPassed = -1;
            var count = 0;
           
            foreach (var test in executionContext.Tests)
            {
                File.WriteAllText(testedCodePath, test.Input);

                var project = new Project(csProjFilePath);
                var didCompile = project.Build();
                project.ProjectCollection.UnloadAllProjects();

                if (!didCompile)
                {
                    result.IsCompiledSuccessfully = false;
                    return result;
                }           

                var processExecutionResult = executor.Execute(
                    this.NUnitConsoleRunnerPath,
                    string.Empty,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    arguments);

                var totalTests = 0;
                var passedTests = 0;

                this.ExtractTestResult(processExecutionResult.ReceivedOutput, out passedTests, out totalTests);
                var message = "Test Passed!";

                if (totalTests == 0)
                {
                    message = "No tests found";
                }
                else if (passedTests == originalTestsPassed)
                {
                    message = "No functionality covering this test!";
                }

                if (count == 0)
                {
                    originalTestsPassed = passedTests;
                    if (totalTests != passedTests)
                    {
                        message = "Not all tests passed on the correct solution.";
                    }
                }

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, message);

                File.Delete(testedCodePath);
                result.TestResults.Add(testResult);
                count++;
            }

            return result;
        }

        private void ExtractTestResult(string receivedOutput, out int passedTests, out int totalTests)
        {
            var testResultsRegex = new Regex(TestResultsRegex);
            var res = testResultsRegex.Match(receivedOutput);
            totalTests = int.Parse(res.Groups[1].Value);
            passedTests = int.Parse(res.Groups[2].Value);
        }

        private void CorrectProjectReferences(Project project)
        {
            project.SetProperty("OutputPath", CompiledResultFolder);

            // Remove the first Project Reference (this should be the reference to the tested project)
            var projectReference = project.GetItems("ProjectReference").FirstOrDefault();
            if (projectReference != null)
            {
                project.RemoveItem(projectReference);
            }

            project.AddItem("Compile", TestedCode);

            var nUnitPrevReference = project.Items.FirstOrDefault(x => x.EvaluatedInclude.Contains("nunit.framework"));
            if (nUnitPrevReference != null)
            {
                project.RemoveItem(nUnitPrevReference);
            }

            // Add our NUnit Reference, if private is false, the .dll will not be copied and the tests will not run
            var nUnitMetaData = new Dictionary<string, string>();
            nUnitMetaData.Add("Private", "True");
            project.AddItem("Reference", NUnitReference, nUnitMetaData);
    
            // If we use NUnit we don't really need the VSTT, it will save us copying of the .dll
            var vsTestFrameworkReference = project.Items
                .FirstOrDefault(x => 
                    x.EvaluatedInclude.Contains("Microsoft.VisualStudio.QualityTools.UnitTestFramework"));

            if (vsTestFrameworkReference != null)
            {
                project.RemoveItem(vsTestFrameworkReference);
            }
        }
    }
}
