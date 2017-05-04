namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Linq;
    using OJS.Common.Extensions;
    using OJS.Common.Models;

    public class JavaSpringAndHibernateProjectExecutionStrategy : JavaProjectTestsExecutionStrategy
    {
        private const string ApplicationPropertiesFileName = "application.properties";

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
            var pathsInZip = FileHelpers.GetFilePathsFromZip(submissionZipFilePath);
            if (pathsInZip.Any(f => f.EndsWith(ApplicationPropertiesFileName)))
            {

            }
        }
    }
}
