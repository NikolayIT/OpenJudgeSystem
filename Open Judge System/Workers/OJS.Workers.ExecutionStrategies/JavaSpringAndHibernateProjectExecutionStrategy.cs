namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;
    using System.Linq;
    using OJS.Common.Extensions;
    using OJS.Common.Models;

    public class JavaSpringAndHibernateProjectExecutionStrategy : JavaProjectTestsExecutionStrategy
    {
        private const string ApplicationPropertiesFileName = "application.properties";
        private const string ResourcesFolderName = "src/main/resources/";

        public JavaSpringAndHibernateProjectExecutionStrategy(
            string javaExecutablePath,
            Func<CompilerType, string> getCompilerPathFunc,
            string javaLibsPath)
            : base(javaExecutablePath, getCompilerPathFunc, javaLibsPath)
        {
        }

        protected override string PrepareSubmissionFile(ExecutionContext context)
        {
            var submissionFilePath = $"{this.WorkingDirectory}\\{SubmissionFileName}";
            File.WriteAllBytes(submissionFilePath, context.FileContent);
            FileHelpers.RemoveFilesFromZip(submissionFilePath, RemoveMacFolderPattern);
            this.ManipulateApplicationProperties(submissionFilePath);
            this.ExtractUserClassNames(submissionFilePath);
            this.AddTestsToUserSubmission(context, submissionFilePath);
            this.AddTestRunnerTemplate(submissionFilePath);

            return submissionFilePath;
        }

        private void ManipulateApplicationProperties(string submissionZipFilePath)
        {
            string fakeApplicationPropertiesPath = $"{this.WorkingDirectory}\\{ApplicationPropertiesFileName}";
            File.WriteAllText(fakeApplicationPropertiesPath, " ");

            var pathsInZip = FileHelpers.GetFilePathsFromZip(submissionZipFilePath);

            string resourceDirectory = pathsInZip.FirstOrDefault(e => e.EndsWith(ResourcesFolderName));

            if (string.IsNullOrEmpty(resourceDirectory))
            {
                throw new FileNotFoundException(
                    $"Resource directory not found in the project!");
            }

            FileHelpers.AddFilesToZipArchive(submissionZipFilePath, resourceDirectory, fakeApplicationPropertiesPath);
            File.Delete(fakeApplicationPropertiesPath);
        }
    }
}
