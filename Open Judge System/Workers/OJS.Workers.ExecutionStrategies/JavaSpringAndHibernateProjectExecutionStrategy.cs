namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml;
    using Checkers;
    using Common;
    using Common.Helpers;
    using Executors;
    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;

    public class JavaSpringAndHibernateProjectExecutionStrategy : JavaProjectTestsExecutionStrategy
    {
        private const string PomXmlFileNameAndExtension = "pom.xml";
        private const string ApplicationPropertiesFileName = "application.properties";
        private const string ResourcesFolderName = "src/main/resources/";
        private const string IntelliJProjectTemplatePattern = "src/main/java";
        private const string IntelliJTestProjectTemplatePattern = "src/test/java";
        private const string PropertySourcePattern = @"(@PropertySources?\((?:.*?)\))";
        private const string PomXmlNamespace = @"http://maven.apache.org/POM/4.0.0";
        private const string StartClassNodeXPath = @"//pomns:properties/pomns:start-class";
        private const string DependencyNodeXPathTemplate = @"//pomns:dependencies/pomns:dependency[pomns:groupId='##' and pomns:artifactId='!!']";
        private const string DependenciesNodeXPath = @"//pomns:dependencies";
        private const string JUnitRunnerConsolePath = @"org.junit.runner.JUnitCore";
        private const string PomXmlBuildSettingsPattern = @"<build>(?s:.)*<\/build>";

        private const string MavenBuildOutputPattern = @"\[INFO\] BUILD (\w+)";
        private const string MavenBuildErrorPattern = @"(?:\[ERROR\] (.*))";

        private static readonly string JUnitFailedTestPattern =
            $@"There was (\d+) failure:{Environment.NewLine}1\) (\w+)\((.+)\){Environment.NewLine}(.+)";

        public JavaSpringAndHibernateProjectExecutionStrategy(
            string javaExecutablePath,
            Func<CompilerType, string> getCompilerPathFunc,
            string javaLibsPath,
            string mavenPath)
            : base(javaExecutablePath, getCompilerPathFunc, javaLibsPath)
        {
            this.MavenPath = mavenPath;
            this.ClassPath = $"-cp {this.JavaLibsPath}*;{this.WorkingDirectory}\\target\\* ";
        }

        public string MavenPath { get; set; }

        public string PackageName { get; set; }

        public string MainClassFileName { get; set; }

        public string ProjectRootDirectoryInSubmissionZip { get; set; }

        public string ProjectTestDirectoryInSubmissionZip { get; set; }

        public string PomXmlBuildSettings => @" <build>
        <plugins>
            <plugin>
                <groupId>org.apache.maven.plugins</groupId>
                <artifactId>maven-compiler-plugin</artifactId>
                <version>3.5.1</version>
                <configuration>
                    <source>1.8</source>
                    <target>1.8</target>
                </configuration>
            </plugin>
            <plugin>
                <artifactId>maven-assembly-plugin</artifactId>
                <configuration>
                    <descriptorRefs>
                        <descriptorRef>jar-with-dependencies</descriptorRef>
                    </descriptorRefs>
                </configuration>
                <executions>
                    <execution>
                        <phase>package</phase>
                        <goals>
                            <goal>single</goal>
                        </goals>
                    </execution>
                </executions>
            </plugin>
            <plugin>
                <groupId>org.apache.maven.plugins</groupId>
                <artifactId>maven-jar-plugin</artifactId>
                <executions>
                    <execution>
                        <goals>
                            <goal>test-jar</goal>
                        </goals>
                        <phase>test-compile</phase>
                    </execution>
                </executions>
                <configuration>
                    <archive>
                        <manifest>
                            <addClasspath>true</addClasspath>
                        </manifest>
                    </archive>
                </configuration>
            </plugin>
        </plugins>
    </build>";

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

            FileHelpers.UnzipFile(submissionFilePath, this.WorkingDirectory);

            string pomXmlPath = FileHelpers.FindFileMatchingPattern(this.WorkingDirectory, PomXmlFileNameAndExtension);

            string[] mavenArgs = new[] { $"-f {pomXmlPath} clean package -o --X" };

            var mavenExecutor = new StandardProcessExecutor();

            var packageExecutionResult = mavenExecutor.Execute(
              this.MavenPath,
              string.Empty,
              executionContext.TimeLimit,
              executionContext.MemoryLimit,
              mavenArgs,
              this.WorkingDirectory);

            Regex mavenBuildOutput = new Regex(MavenBuildOutputPattern);
            Regex mavenBuildErrors = new Regex(MavenBuildErrorPattern);

            Match compilationMatch = mavenBuildOutput.Match(packageExecutionResult.ReceivedOutput);
            Match errorMatch = mavenBuildErrors.Match(packageExecutionResult.ReceivedOutput);
            result.IsCompiledSuccessfully = compilationMatch.Groups[1].Value == "SUCCESS";
            result.CompilerComment = $"{errorMatch.Groups[1]}";

            if (!result.IsCompiledSuccessfully)
            {
                return result;
            }

            var restrictedExe = new RestrictedProcessExecutor();

            var arguments = new List<string>();
            arguments.Add(this.ClassPath);
            arguments.Add(AdditionalExecutionArguments);
            arguments.Add(JUnitRunnerConsolePath);

            Regex testErrorMatcher = new Regex(JUnitFailedTestPattern);
            var checker = Checker.CreateChecker(
              executionContext.CheckerAssemblyName,
              executionContext.CheckerTypeName,
              executionContext.CheckerParameter);
            var testIndex = 0;

            foreach (var test in executionContext.Tests)
            {
                var testFile = this.TestNames[testIndex++];
                arguments.Add(testFile);

                var processExecutionResult = restrictedExe.ExecuteJavaProcess(
                this.JavaExecutablePath,
                string.Empty,
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                this.WorkingDirectory,
                arguments);

                arguments.Remove(testFile);

                if (processExecutionResult.ReceivedOutput.Contains(JvmInsufficientMemoryMessage))
                {
                    throw new InsufficientMemoryException(JvmInsufficientMemoryMessage);
                }

                string message = this.EvaluateJUnitOutput(processExecutionResult.ReceivedOutput, testErrorMatcher);

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, message);
                result.TestResults.Add(testResult);
            }

            return result;
        }

        protected string EvaluateJUnitOutput(string testOutput, Regex testErrorMatcher)
        {
            string message = "Test Passed!";
            MatchCollection errorMatches = testErrorMatcher.Matches(testOutput);
         
            if (errorMatches.Count > 0)
            {
                Match lastMatch = errorMatches[errorMatches.Count - 1];
                string errorMethod = lastMatch.Groups[1].Value;
                string className = lastMatch.Groups[2].Value;
                string errorReason = lastMatch.Groups[3].Value;
                message = $"Failed test fixture: {className} at {errorMethod}; {errorReason}";
            }

            return message;
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
            this.PreparePomXml(submissionFilePath);

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

            XmlNode packageName = rootNode.SelectSingleNode(StartClassNodeXPath, namespaceManager);

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
                File.WriteAllText(testFileName, $"package {this.PackageName};{Environment.NewLine}{test.Input}");
                filePaths[testNumber] = testFileName;
                this.TestNames.Add($"{this.PackageName}.{className}");
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
            string pomXmlFolderPathInZip = Path
                .GetDirectoryName(FileHelpers
                                 .GetFilePathsFromZip(submissionFilePath)
                                 .FirstOrDefault(f => f.EndsWith(this.MainClassFileName)));

            FileHelpers.AddFilesToZipArchive(submissionFilePath, pomXmlFolderPathInZip, mainClassFilePath);
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

            var pathsInZip = FileHelpers.GetFilePathsFromZip(submissionZipFilePath);

            string resourceDirectory = Path.GetDirectoryName(pathsInZip.FirstOrDefault(f => f.EndsWith(ApplicationPropertiesFileName)));

            if (string.IsNullOrEmpty(resourceDirectory))
            {
                throw new FileNotFoundException(
                    $"Resource directory not found in the project!");
            }

            FileHelpers.AddFilesToZipArchive(submissionZipFilePath, resourceDirectory, fakeApplicationPropertiesPath);
            File.Delete(fakeApplicationPropertiesPath);
        }

        private void PreparePomXml(string submissionFilePath)
        {
            string extractionDirectory = DirectoryHelpers.CreateTempDirectory();

            string pomXmlFilePath = FileHelpers.ExtractFileFromZip(
                submissionFilePath,
                PomXmlFileNameAndExtension,
            extractionDirectory);

            if (string.IsNullOrEmpty(pomXmlFilePath))
            {
                throw new FileNotFoundException("Pom.xml not found in submission!");
            }

            this.AddBuildSettings(pomXmlFilePath);
            this.AddDependencies(pomXmlFilePath);
            string mainClassFolderPathInZip = Path
                .GetDirectoryName(FileHelpers
                                 .GetFilePathsFromZip(submissionFilePath)
                                 .FirstOrDefault(f => f.EndsWith(PomXmlFileNameAndExtension)));

            FileHelpers.AddFilesToZipArchive(submissionFilePath, mainClassFolderPathInZip, pomXmlFilePath);
            DirectoryHelpers.SafeDeleteDirectory(extractionDirectory, true);
        }

        private void AddDependencies(string pomXmlFilePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(pomXmlFilePath);

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("pomns", PomXmlNamespace);

            XmlNode rootNode = doc.DocumentElement;
            if (rootNode == null)
            {
                throw new XmlException("Root element not specified in pom.xml");
            }

            // groupId -> artifactId
            Dictionary<string, Tuple<string, string>> dependenciesToInsert =
                new Dictionary<string, Tuple<string, string>>()
            {
                { "javax.el", new Tuple<string, string>( "el-api", "2.2") },
                { "junit", new Tuple<string, string>( "junit", null) },
                { "org.hsqldb", new Tuple<string, string>( "hsqldb", null) },
                {"org.springframework.boot", new Tuple<string, string>("spring-boot-starter-test", "1.5.2.RELEASE") }
            };

            XmlNode dependenciesNode = rootNode.SelectSingleNode(DependenciesNodeXPath, namespaceManager);
            if (dependenciesNode == null)
            {
                throw new XmlException("No dependencies specified in pom.xml");
            }

            foreach (KeyValuePair<string, Tuple<string, string>> groupIdArtifactId in dependenciesToInsert)
            {
                XmlNode dependencyNode = rootNode
                    .SelectSingleNode(
                     DependencyNodeXPathTemplate
                    .Replace("##", groupIdArtifactId.Key).Replace("!!", groupIdArtifactId.Value.Item1), namespaceManager);
                if (dependencyNode == null)
                {
                    dependencyNode = doc.CreateNode(XmlNodeType.Element, "dependency", PomXmlNamespace);

                    XmlNode groupId = doc.CreateNode(XmlNodeType.Element, "groupId", PomXmlNamespace);
                    groupId.InnerText = groupIdArtifactId.Key;
                    XmlNode artifactId = doc.CreateNode(XmlNodeType.Element, "artifactId", PomXmlNamespace);
                    artifactId.InnerText = groupIdArtifactId.Value.Item1;

                    if (groupIdArtifactId.Value.Item2 != null)
                    {
                        XmlNode versionNumber = doc.CreateNode(XmlNodeType.Element, "version", PomXmlNamespace);
                        versionNumber.InnerText = groupIdArtifactId.Value.Item2;
                        dependencyNode.AppendChild(versionNumber);
                    }

                    dependencyNode.AppendChild(groupId);
                    dependencyNode.AppendChild(artifactId);
                    dependenciesNode.AppendChild(dependencyNode);
                }
            }

            doc.Save(pomXmlFilePath);
        }

        private void AddBuildSettings(string pomXmlFilePath)
        {
            string pomXmlContent = File.ReadAllText(pomXmlFilePath);
            Regex buildSettingsRegex = new Regex(PomXmlBuildSettingsPattern);
            if (buildSettingsRegex.IsMatch(pomXmlContent))
            {
                pomXmlContent = Regex.Replace(pomXmlContent, PomXmlBuildSettingsPattern, this.PomXmlBuildSettings);
            }

            File.WriteAllText(pomXmlFilePath, pomXmlContent);
        }
    }
}
