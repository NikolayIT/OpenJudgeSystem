namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;
    using OJS.Common;
    using OJS.Common.Extensions;

    public class PhpProjectExecutionStrategy : ExecutionStrategy
    {
        private const string ZippedSubmissionName = "_$Submission";
        private const string SuperGlobalsTemplateName = "_Superglobals.php";
        private const string SubmissionEntryPoint = "index.php";
        private const string SuperGlobalsRequireStatementTemplate = "<?php require_once '##templateName##'; ?>";
        private readonly string phpCliExecutablePath;

        public PhpProjectExecutionStrategy(string phpCliExecutablePath)
        {
            if (!File.Exists(phpCliExecutablePath))
            {
                throw new ArgumentException($"PHP CLI not found in: {phpCliExecutablePath}", nameof(phpCliExecutablePath));
            }

            this.phpCliExecutablePath = phpCliExecutablePath;
        }

        public string SuperGlobalsTemplatePath => $"{this.WorkingDirectory}\\${SuperGlobalsTemplateName}";

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

            string pathToSubmissionEntryPoint =
                FileHelpers.FindFileMatchingPattern(this.WorkingDirectory, SubmissionEntryPoint);

            if (string.IsNullOrEmpty(pathToSubmissionEntryPoint))
            {
                throw new ArgumentException($"{SubmissionEntryPoint} not found in submission folder!");
            }

            this.RequireSuperGlobalsTemplateInUserCode(pathToSubmissionEntryPoint);
            foreach (var test in executionContext.Tests)
            {
                File.WriteAllText(this.SuperGlobalsTemplatePath, test.Input);
            }

            return result;
        }

        private void RequireSuperGlobalsTemplateInUserCode(string pathToSubmissionEntryPoint)
        {
            string entryPointContents = File.ReadAllText(pathToSubmissionEntryPoint);

            string requireSuperGlobalsStatement =
                SuperGlobalsRequireStatementTemplate.Replace("##templateName##", SuperGlobalsTemplateName);
            entryPointContents = $"{requireSuperGlobalsStatement}{Environment.NewLine}{entryPointContents}";

            File.WriteAllText(pathToSubmissionEntryPoint, entryPointContents);
        }
    }
}
