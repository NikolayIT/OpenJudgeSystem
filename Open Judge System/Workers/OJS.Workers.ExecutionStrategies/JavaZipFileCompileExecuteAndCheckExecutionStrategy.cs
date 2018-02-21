namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;

    using Ionic.Zip;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Workers.Common;

    public class JavaZipFileCompileExecuteAndCheckExecutionStrategy : JavaPreprocessCompileExecuteAndCheckExecutionStrategy
    {
        protected const string SubmissionFileName = "_$submission";

        public JavaZipFileCompileExecuteAndCheckExecutionStrategy(
            string javaExecutablePath,
            Func<CompilerType, string> getCompilerPathFunc,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(javaExecutablePath, getCompilerPathFunc, baseTimeUsed, baseMemoryUsed)
        {
        }

        protected override string CreateSubmissionFile(ExecutionContext executionContext)
        {
            var trimmedAllowedFileExtensions = executionContext.AllowedFileExtensions?.Trim();

            var allowedFileExtensions = (!trimmedAllowedFileExtensions?.StartsWith(".") ?? false)
                ? $".{trimmedAllowedFileExtensions}"
                : trimmedAllowedFileExtensions;

            if (allowedFileExtensions != GlobalConstants.ZipFileExtension)
            {
                throw new ArgumentException("Submission file is not a zip file!");
            }

            return this.PrepareSubmissionFile(executionContext.FileContent);
        }

        protected override CompileResult DoCompile(ExecutionContext executionContext, string submissionFilePath)
        {
            var compilerPath = this.GetCompilerPathFunc(executionContext.CompilerType);

            // Compile the zip file with user code and sandbox executor
            var compilerResult = this.Compile(
                executionContext.CompilerType,
                compilerPath,
                executionContext.AdditionalCompilerArguments,
                submissionFilePath);

            return compilerResult;
        }

        private string PrepareSubmissionFile(byte[] submissionFileContent)
        {
            var submissionFilePath = $"{this.WorkingDirectory}\\{SubmissionFileName}";
            File.WriteAllBytes(submissionFilePath, submissionFileContent);
            FileHelpers.RemoveFilesFromZip(submissionFilePath, RemoveMacFolderPattern);
            this.AddSandboxExecutorSourceFileToSubmissionZip(submissionFilePath);
            return submissionFilePath;
        }

        private void AddSandboxExecutorSourceFileToSubmissionZip(string submissionZipFilePath)
        {
            using (var zipFile = new ZipFile(submissionZipFilePath))
            {
                zipFile.AddFile(this.SandboxExecutorSourceFilePath, string.Empty);

                zipFile.Save();
            }
        }
    }
}