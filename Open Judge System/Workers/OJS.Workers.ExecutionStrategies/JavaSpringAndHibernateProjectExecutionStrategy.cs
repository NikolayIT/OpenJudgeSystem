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
            this.ManipulateApplicationProperties(submissionFilePath);
            return base.PrepareSubmissionFile(context);
        }

        private void ManipulateApplicationProperties(string submissionZipFilePath)
        {
            string fakeApplicationPropertiesPath = $"{this.WorkingDirectory}\\{ApplicationPropertiesFileName}";
            File.WriteAllText(fakeApplicationPropertiesPath, string.Empty);

            var pathsInZip = FileHelpers.GetFilePathsFromZip(submissionZipFilePath).ToList();
            string resourceDirectory = pathsInZip.FirstOrDefault(e => e.EndsWith(ResourcesFolderName));

            if (string.IsNullOrEmpty(resourceDirectory))
            {
                throw new FileNotFoundException(
                    $"{0} not found in the project!", "Resource folder");
            }

            FileHelpers.AddFilesToZipArchive(submissionZipFilePath, resourceDirectory, fakeApplicationPropertiesPath);
            File.Delete(fakeApplicationPropertiesPath);
        }
    }
}
