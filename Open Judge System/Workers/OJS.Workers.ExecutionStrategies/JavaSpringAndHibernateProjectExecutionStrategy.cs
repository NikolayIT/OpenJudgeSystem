namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Common.Helpers;
    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;

    public class JavaSpringAndHibernateProjectExecutionStrategy : JavaProjectTestsExecutionStrategy
    {
        private const string ApplicationPropertiesFileName = "application.properties";
        private const string ResourcesFolderName = "src/main/resources/";
        private const string MainClassFileName = "main.java";
        private const string IntelliJTemplateFoldersPattern = "main/java/com/photographyworkshops";
        private const string IntelliJTestTemplateFoldersPattern = @"test/java/com/photographyworkshops/test";
        private const string PropertySourcePattern = @"(@PropertySources?\((?:.*?)\))";
        private const string JavaSourceFolder = "src";

        public JavaSpringAndHibernateProjectExecutionStrategy(
            string javaExecutablePath,
            Func<CompilerType, string> getCompilerPathFunc,
            string javaLibsPath)
            : base(javaExecutablePath, getCompilerPathFunc, javaLibsPath)
        {
        }

        public string ProjectRootDirectoryInSubmissionZip { get; set; } =
            $"{JavaSourceFolder}/{IntelliJTemplateFoldersPattern}";

        protected override string PrepareSubmissionFile(ExecutionContext context)
        {
            var submissionFilePath = $"{this.WorkingDirectory}\\{SubmissionFileName}";
            File.WriteAllBytes(submissionFilePath, context.FileContent);
            FileHelpers.RemoveFilesFromZip(submissionFilePath, RemoveMacFolderPattern);

            this.OverwriteApplicationProperties(submissionFilePath);
            this.RemovePropertySourceAnnotationsFromMainClass(submissionFilePath);
            this.ExtractUserClassNames(submissionFilePath);
            this.AddTestsToUserSubmission(context, submissionFilePath);
            this.AddTestRunnerTemplate(submissionFilePath);

            return submissionFilePath;
        }

        protected override void AddTestRunnerTemplate(string submissionFilePath)
        {
            File.WriteAllText(this.JUnitTestRunnerSourceFilePath, this.JUnitTestRunnerCode);
            FileHelpers.AddFilesToZipArchive(
                submissionFilePath, 
                this.ProjectRootDirectoryInSubmissionZip, 
                this.JUnitTestRunnerSourceFilePath);
            FileHelpers.DeleteFiles(this.JUnitTestRunnerSourceFilePath);
        }

        protected override void AddTestsToUserSubmission(ExecutionContext context, string submissionZipFilePath)
        {
            var testNumber = 0;
            var filePaths = new string[context.Tests.Count()];

            foreach (var test in context.Tests)
            {
                var className = JavaCodePreprocessorHelper.GetPublicClassName(test.Input);
                var testFileName =
                        $"{this.WorkingDirectory}\\{className}{GlobalConstants.JavaSourceFileExtension}";
                File.WriteAllText(testFileName, test.Input);
                filePaths[testNumber] = testFileName;
                this.TestNames.Add(className);
                testNumber++;
            }

            FileHelpers.AddFilesToZipArchive(
                submissionZipFilePath,
                this.ProjectRootDirectoryInSubmissionZip, 
                filePaths);
            FileHelpers.DeleteFiles(filePaths);
        }

        protected override void ExtractUserClassNames(string submissionFilePath)
        {
            this.UserClassNames.AddRange(
                FileHelpers.GetFilePathsFromZip(submissionFilePath)
                    .Where(x => !x.EndsWith("/") && x.EndsWith(GlobalConstants.JavaSourceFileExtension))
                     .Select(x => x.Contains(IntelliJTemplateFoldersPattern)
                                ? x.Substring(x.LastIndexOf(
                                    IntelliJTemplateFoldersPattern,
                                    StringComparison.Ordinal)
                                    + IntelliJTemplateFoldersPattern.Length
                                    + 1)
                                : x)
                    .Select(x => x.Contains(".") ? x.Substring(0, x.LastIndexOf(".", StringComparison.Ordinal)) : x)
                    .Select(x => x.Replace("/", ".")));
        }

        private void RemovePropertySourceAnnotationsFromMainClass(string submissionFilePath)
        {
            string extractionDirectory = DirectoryHelpers.CreateTempDirectory();

            string mainClassFilePath = FileHelpers.ExtractFileFromZip(
                submissionFilePath,
                MainClassFileName,
            extractionDirectory);

            string mainClassContent = File.ReadAllText(mainClassFilePath);

            Regex propertySourceMatcher = new Regex(PropertySourcePattern);
            while (propertySourceMatcher.IsMatch(mainClassContent))
            {
                mainClassContent = Regex.Replace(mainClassContent, PropertySourcePattern, string.Empty);
            }

            File.WriteAllText(mainClassFilePath, mainClassContent);
            string mainClassFolderPathInZip = Path
                .GetDirectoryName(FileHelpers
                                 .GetFilePathsFromZip(submissionFilePath)
                                 .FirstOrDefault(f => f.EndsWith(MainClassFileName)));

            FileHelpers.AddFilesToZipArchive(submissionFilePath, mainClassFolderPathInZip, mainClassFilePath);
            DirectoryHelpers.SafeDeleteDirectory(extractionDirectory, true);
        }

        private void OverwriteApplicationProperties(string submissionZipFilePath)
        {
            string fakeApplicationPropertiesPath = $"{this.WorkingDirectory}\\{ApplicationPropertiesFileName}";
            File.WriteAllText(fakeApplicationPropertiesPath, @"spring.jpa.hibernate.ddl-auto=create-drop
spring.jpa.database=HSQL
#spring.jpa.properties.hibernate.dialect=org.hibernate.dialect.HSQLDialect
spring.datasource.driverClassName=org.hsqldb.jdbcDriver
spring.datasource.url=jdbc:hsqldb:mem:.
spring.datasource.username=sa
spring.datasource.password=

security.basic.enabled=false");

            var pathsInZip = FileHelpers.GetFilePathsFromZip(submissionZipFilePath);

            string resourceDirectory = pathsInZip.FirstOrDefault(f => f.EndsWith(ResourcesFolderName));

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
