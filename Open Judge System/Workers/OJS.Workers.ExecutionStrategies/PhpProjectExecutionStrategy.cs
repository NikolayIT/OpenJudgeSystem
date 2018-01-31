namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Workers.Checkers;
    using OJS.Workers.Executors;

    public class PhpProjectExecutionStrategy : ExecutionStrategy
    {
        protected const string ZippedSubmissionName = "_$Submission";
        protected const string ApplicationEntryPoint = "index.php";
        protected readonly string phpCliExecutablePath;
        private const string SuperGlobalsTemplateName = "_Superglobals.php";
        private const string SuperGlobalsRequireStatementTemplate = "<?php require_once '##templateName##'; ?>";

        public PhpProjectExecutionStrategy(
            string phpCliExecutablePath,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(baseTimeUsed, baseMemoryUsed)
        {
            if (!File.Exists(phpCliExecutablePath))
            {
                throw new ArgumentException(
                    $"PHP CLI not found in: {phpCliExecutablePath}", 
                    nameof(phpCliExecutablePath));
            }

            this.phpCliExecutablePath = phpCliExecutablePath;
        }

        public string SuperGlobalsTemplatePath => $"{this.WorkingDirectory}\\{SuperGlobalsTemplateName}";

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            // PHP code is not compiled
            result.IsCompiledSuccessfully = true;

            string submissionPath = 
                $@"{this.WorkingDirectory}\\{ZippedSubmissionName}{GlobalConstants.ZipFileExtension}";
            File.WriteAllBytes(submissionPath, executionContext.FileContent);
            FileHelpers.UnzipFile(submissionPath, this.WorkingDirectory);
            File.Delete(submissionPath);

            string applicationEntryPointPath =
                FileHelpers.FindFileMatchingPattern(this.WorkingDirectory, ApplicationEntryPoint);

            if (string.IsNullOrEmpty(applicationEntryPointPath))
            {
                throw new ArgumentException($"{ApplicationEntryPoint} not found in submission folder!");
            }

            this.RequireSuperGlobalsTemplateInUserCode(applicationEntryPointPath);

            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName, 
                executionContext.CheckerTypeName, 
                executionContext.CheckerParameter);

            result.TestResults = new List<TestResult>();

            var executor = new RestrictedProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);
            foreach (var test in executionContext.Tests)
            {
                File.WriteAllText(this.SuperGlobalsTemplatePath, test.Input);

                var processExecutionResult = executor.Execute(
                    this.phpCliExecutablePath,
                    string.Empty,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    new[] { applicationEntryPointPath });

                var testResult = this.ExecuteAndCheckTest(
                    test, 
                    processExecutionResult, 
                    checker, 
                    processExecutionResult.ReceivedOutput);

                result.TestResults.Add(testResult);
            }

            return result;
        }

        protected void RequireSuperGlobalsTemplateInUserCode(string pathToSubmissionEntryPoint)
        {
            string entryPointContents = File.ReadAllText(pathToSubmissionEntryPoint);

            string requireSuperGlobalsStatement =
                SuperGlobalsRequireStatementTemplate.Replace("##templateName##", SuperGlobalsTemplateName);
            entryPointContents = $"{requireSuperGlobalsStatement}{Environment.NewLine}{entryPointContents}";

            File.WriteAllText(pathToSubmissionEntryPoint, entryPointContents);
        }
    }
}
