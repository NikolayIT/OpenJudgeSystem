namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml;
    using Checkers;
    using Common.Helpers;
    using Executors;
    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;

    public class JavaSpringAndHibernateProjectExecutionStrategy : JavaProjectTestsExecutionStrategy
    {
        private const string ApplicationPropertiesFileName = "application.properties";
        private const string ResourcesFolderName = "src/main/resources/";
        private const string IntelliJProjectTemplatePattern = "src/main/java";
        private const string IntelliJTestProjectTemplatePattern = "src/test/java";
        private const string PropertySourcePattern = @"(@PropertySources?\((?:.*?)\))";
        private const string PomXmlNamespace = @"http://maven.apache.org/POM/4.0.0";
        private const string StartClassNodePath = @"//pomns:properties/pomns:start-class";


        public JavaSpringAndHibernateProjectExecutionStrategy(
            string javaExecutablePath,
            Func<CompilerType, string> getCompilerPathFunc,
            string javaLibsPath,
            string mavenPath)
            : base(javaExecutablePath, getCompilerPathFunc, javaLibsPath)
        {
            this.MavenPath = mavenPath;
        }

        public string MavenPath { get; set; }

        public string PackageName { get; set; }

        public string MainClassFileName { get; set; }

        public string ProjectRootDirectoryInSubmissionZip { get; set; }

        public string ProjectTestDirectoryInSubmissionZip { get; set; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            // Create a temp file with the submission code
            string submissionFilePath;
            try
            {
                submissionFilePath = this.CreateSubmissionFile(executionContext);
            }
            catch (ArgumentException exception)
            {
                result.IsCompiledSuccessfully = false;
                result.CompilerComment = exception.Message;

                return result;
            }

            // var executor = new StandardProcessExecutor();
            var executor = new RestrictedProcessExecutor();
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            FileHelpers.UnzipFile(submissionFilePath, this.WorkingDirectory);

            string pomXmlPath = FileHelpers.FindFileMatchingPattern(this.WorkingDirectory, "pom.xml");

            string[] mavenArgs = new[] { $"-f {pomXmlPath} clean test -Dtest={string.Join(",", this.TestNames)}" };

            var processExecutionResult = executor.Execute(
              this.MavenPath,
              string.Empty,
              executionContext.TimeLimit,
              executionContext.MemoryLimit,
              mavenArgs,
              this.WorkingDirectory);
            return null;

        }

        protected override string PrepareSubmissionFile(ExecutionContext context)
        {
            var submissionFilePath = $"{this.WorkingDirectory}\\{SubmissionFileName}";
            File.WriteAllBytes(submissionFilePath, context.FileContent);
            FileHelpers.RemoveFilesFromZip(submissionFilePath, RemoveMacFolderPattern);

            this.ExtractPackageAndDirectoryNames(submissionFilePath);
            this.OverwriteApplicationProperties(submissionFilePath);
            this.RemovePropertySourceAnnotationsFromMainClass(submissionFilePath);
            this.AddTestsToUserSubmission(context, submissionFilePath);

            return submissionFilePath;
        }

        protected void ExtractPackageAndDirectoryNames(string submissionFilePath)
        {
            this.MainClassFileName = this.ExtractEntryPointFromPomXml(submissionFilePath);

            this.PackageName = this.MainClassFileName
                .Substring(0, this.MainClassFileName.LastIndexOf(".", StringComparison.Ordinal));

            string normalizedPath = this.PackageName.Replace(".", "/");

            this.ProjectRootDirectoryInSubmissionZip = $"{IntelliJProjectTemplatePattern}/{normalizedPath}/";
            this.ProjectTestDirectoryInSubmissionZip = $"{IntelliJTestProjectTemplatePattern}/{normalizedPath}/";

            this.MainClassFileName = this.MainClassFileName
                                         .Substring(this.MainClassFileName
                                                    .LastIndexOf(".", StringComparison.Ordinal) + 1)
                                                    + GlobalConstants.JavaSourceFileExtension;
        }

        protected string ExtractEntryPointFromPomXml(string submissionFilePath)
        {
            string pomXmlPath = FileHelpers.ExtractFileFromZip(submissionFilePath, "pom.xml", this.WorkingDirectory);

            if (string.IsNullOrEmpty(pomXmlPath))
            {
                throw new ArgumentException($"{nameof(pomXmlPath)} was not found in submission!");
            }

            XmlDocument pomXml = new XmlDocument();
            pomXml.Load(pomXmlPath);

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(pomXml.NameTable);
            namespaceManager.AddNamespace("pomns", PomXmlNamespace);

            XmlNode rootNode = pomXml.DocumentElement;

            XmlNode packageName = rootNode.SelectSingleNode(StartClassNodePath, namespaceManager);

            if (packageName == null)
            {
                throw new ArgumentException($"Starter path not defined in pom.xml!");
            }

            FileHelpers.DeleteFiles(pomXmlPath);
            return packageName.InnerText.Trim();
        }

        protected override void AddTestRunnerTemplate(string submissionFilePath)
        {
            File.WriteAllText(this.JUnitTestRunnerSourceFilePath, this.JUnitTestRunnerCode);
            FileHelpers.AddFilesToZipArchive(
                submissionFilePath,
                this.ProjectTestDirectoryInSubmissionZip,
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
                File.WriteAllText(testFileName, $"package {PackageName};{Environment.NewLine}{test.Input}");
                filePaths[testNumber] = testFileName;
                this.TestNames.Add(className);
                testNumber++;
            }

            FileHelpers.AddFilesToZipArchive(
                submissionZipFilePath,
                this.ProjectTestDirectoryInSubmissionZip,
                filePaths);
            FileHelpers.DeleteFiles(filePaths);
        }

        protected override void ExtractUserClassNames(string submissionFilePath)
        {
            this.UserClassNames.AddRange(
                FileHelpers.GetFilePathsFromZip(submissionFilePath)
                    .Where(x => !x.EndsWith("/") && x.EndsWith(GlobalConstants.JavaSourceFileExtension))
                     .Select(x => x.Contains(IntelliJProjectTemplatePattern)
                                ? x.Substring(x.LastIndexOf(
                                    IntelliJProjectTemplatePattern,
                                    StringComparison.Ordinal)
                                    + IntelliJProjectTemplatePattern.Length
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
                this.MainClassFileName,
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
            spring.main.web-environment=false
            security.basic.enabled=false");
            //   File.WriteAllText(fakeApplicationPropertiesPath, " ");

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
