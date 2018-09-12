namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml;

    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Common.Models;
    using OJS.Workers.ExecutionStrategies.Helpers;
    using OJS.Workers.Executors;

    public class JavaSpringAndHibernateProjectExecutionStrategy : JavaProjectTestsExecutionStrategy
    {
        private const string PomXmlFileNameAndExtension = "pom.xml";
        private const string ApplicationPropertiesFileName = "application.properties";
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
        private static readonly string JUnitFailedTestPattern =
            $@"There was (?:\d+) failure:{Environment.NewLine}1\) (\w+)\((.+)\){Environment.NewLine}(.+)";

        private static readonly string MavenBuildErrorPattern = @"\[ERROR\](?:\s)*((?:.*)\n|\r|(\r\n))*(?=\[INFO\]\s\d)";

        public JavaSpringAndHibernateProjectExecutionStrategy(
            string javaExecutablePath,
            Func<CompilerType, string> getCompilerPathFunc,
            string javaLibrariesPath,
            string mavenPath,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(
                javaExecutablePath,
                getCompilerPathFunc,
                javaLibrariesPath,
                baseTimeUsed,
                baseMemoryUsed) =>
                    this.MavenPath = mavenPath;

        // Property contains Dictionary<GroupId, Tuple<ArtifactId, Version>>
        public Dictionary<string, Tuple<string, string>> Dependencies =>
            new Dictionary<string, Tuple<string, string>>()
            {
                { "javax.el", new Tuple<string, string>("el-api", "2.2") },
                { "junit", new Tuple<string, string>("junit", null) },
                { "org.hsqldb", new Tuple<string, string>("hsqldb", null) },
                { "org.springframework.boot", new Tuple<string, string>("spring-boot-starter-test", "1.5.2.RELEASE") },
                { "com.sun.xml.bind", new Tuple<string, string>("jaxb-impl", "2.2.7") }
            };

        protected string MavenPath { get; set; }

        protected string PackageName { get; set; }

        protected string MainClassFileName { get; set; }

        protected string ProjectRootDirectoryInSubmissionZip { get; set; }

        protected string ProjectTestDirectoryInSubmissionZip { get; set; }

        protected string PomXmlBuildSettings => @" <build>
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
            <plugin>
                <groupId>org.codehaus.mojo</groupId>
                <artifactId>build-helper-maven-plugin</artifactId>
                <version>1.7</version>
                <executions>
                    <execution>
                        <id>remove-old-artifacts</id>
                        <phase>package</phase>
                        <goals>
                            <goal>remove-project-artifact</goal>
                        </goals>
                        <configuration>
                            <removeAll>true</removeAll><!-- When true, remove all built artifacts including all versions. When false, remove all built artifacts of this project version -->
                        </configuration>
                    </execution>
                </executions>
            </plugin>
        </plugins>
    </build>";

        protected override string ClassPath => $"-cp {this.JavaLibrariesPath}*;{this.WorkingDirectory}\\target\\* ";

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

            var pomXmlPath = FileHelpers.FindFileMatchingPattern(this.WorkingDirectory, PomXmlFileNameAndExtension);

            var mavenArgs = new[] { $"-f {pomXmlPath} clean package -DskipTests" };

            var mavenExecutor = new StandardProcessExecutor();

            var packageExecutionResult = mavenExecutor.Execute(
              this.MavenPath,
              string.Empty,
              executionContext.TimeLimit,
              executionContext.MemoryLimit,
              mavenArgs,
              this.WorkingDirectory);

            var mavenBuildOutput = new Regex(MavenBuildOutputPattern);
            var compilationMatch = mavenBuildOutput.Match(packageExecutionResult.ReceivedOutput);

            result.IsCompiledSuccessfully = compilationMatch.Groups[1].Value == "SUCCESS";

            if (!result.IsCompiledSuccessfully)
            {
                var mavenBuildErrors = new Regex(MavenBuildErrorPattern);
                var errorMatch = mavenBuildErrors.Match(packageExecutionResult.ReceivedOutput);
                result.CompilerComment = $"{errorMatch.Groups[0]}";
                return result;
            }

            var executor = new RestrictedProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);

            var arguments = new List<string>
            {
                this.ClassPath,
                AdditionalExecutionArguments,
                JUnitRunnerConsolePath
            };

            var testErrorMatcher = new Regex(JUnitFailedTestPattern);
            var checker = Checker.CreateChecker(
              executionContext.CheckerAssemblyName,
              executionContext.CheckerTypeName,
              executionContext.CheckerParameter);
            var testIndex = 0;

            foreach (var test in executionContext.Tests)
            {
                var testFile = this.TestNames[testIndex++];
                arguments.Add(testFile);

                var processExecutionResult = executor.Execute(
                this.JavaExecutablePath,
                string.Empty,
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                arguments,
                this.WorkingDirectory);

                JavaStrategiesHelper.ValidateJvmInitialization(processExecutionResult.ReceivedOutput);

                if (processExecutionResult.ReceivedOutput.Contains($"Could not find class: {testFile}"))
                {
                    throw new FileLoadException("Tests could not be loaded, project structure is incorrect");
                }

                var message = this.EvaluateJUnitOutput(processExecutionResult.ReceivedOutput, testErrorMatcher);

                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, message);
                result.TestResults.Add(testResult);

                arguments.Remove(testFile);
            }

            return result;
        }

        protected string EvaluateJUnitOutput(string testOutput, Regex testErrorMatcher)
        {
            var message = "Test Passed!";
            var errorMatches = testErrorMatcher.Matches(testOutput);

            if (errorMatches.Count > 0)
            {
                var lastMatch = errorMatches[errorMatches.Count - 1];
                var errorMethod = lastMatch.Groups[1].Value;
                var className = lastMatch.Groups[2].Value;
                var errorReason = lastMatch.Groups[3].Value;
                message = $"Failed test fixture: {errorReason} in CLASS: {className} at METHOD: {errorMethod}";
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

            var normalizedPath = this.PackageName.Replace(".", "/");

            this.ProjectRootDirectoryInSubmissionZip = $"{IntelliJProjectTemplatePattern}/{normalizedPath}/";
            this.ProjectTestDirectoryInSubmissionZip = $"{IntelliJTestProjectTemplatePattern}/{normalizedPath}/";

            this.MainClassFileName = this.MainClassFileName.Substring(
                this.MainClassFileName.LastIndexOf(".", StringComparison.Ordinal) + 1)
                + Constants.JavaSourceFileExtension;
        }

        protected void OverwriteApplicationProperties(string submissionZipFilePath)
        {
            var fakeApplicationPropertiesPath = $"{this.WorkingDirectory}\\{ApplicationPropertiesFileName}";
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

            var resourceDirectory = Path.GetDirectoryName(pathsInZip.FirstOrDefault(f => f.EndsWith(ApplicationPropertiesFileName)));

            if (string.IsNullOrEmpty(resourceDirectory))
            {
                throw new FileNotFoundException(
                    $"Resource directory not found in the project!");
            }

            FileHelpers.AddFilesToZipArchive(submissionZipFilePath, resourceDirectory, fakeApplicationPropertiesPath);
            File.Delete(fakeApplicationPropertiesPath);
        }

        protected void RemovePropertySourceAnnotationsFromMainClass(string submissionFilePath)
        {
            var extractionDirectory = DirectoryHelpers.CreateTempDirectoryForExecutionStrategy();

            var mainClassFilePath = FileHelpers.ExtractFileFromZip(
                submissionFilePath,
                this.MainClassFileName,
            extractionDirectory);

            var mainClassContent = File.ReadAllText(mainClassFilePath);

            var propertySourceMatcher = new Regex(PropertySourcePattern);
            while (propertySourceMatcher.IsMatch(mainClassContent))
            {
                mainClassContent = Regex.Replace(mainClassContent, PropertySourcePattern, string.Empty);
            }

            File.WriteAllText(mainClassFilePath, mainClassContent);
            var pomXmlFolderPathInZip = Path.GetDirectoryName(FileHelpers
                .GetFilePathsFromZip(submissionFilePath)
                .FirstOrDefault(f => f.EndsWith(this.MainClassFileName)));

            FileHelpers.AddFilesToZipArchive(submissionFilePath, pomXmlFolderPathInZip, mainClassFilePath);
            DirectoryHelpers.SafeDeleteDirectory(extractionDirectory, true);
        }

        protected override void AddTestsToUserSubmission(ExecutionContext context, string submissionZipFilePath)
        {
            var testNumber = 0;
            var filePaths = new string[context.Tests.Count()];

            foreach (var test in context.Tests)
            {
                var className = JavaCodePreprocessorHelper.GetPublicClassName(test.Input);
                var testFileName =
                        $"{this.WorkingDirectory}\\{className}{Constants.JavaSourceFileExtension}";
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
            this.UserClassNames.AddRange(FileHelpers
                .GetFilePathsFromZip(submissionFilePath)
                .Where(x => !x.EndsWith("/") && x.EndsWith(Constants.JavaSourceFileExtension))
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

        protected void PreparePomXml(string submissionFilePath)
        {
            var extractionDirectory = DirectoryHelpers.CreateTempDirectoryForExecutionStrategy();

            var pomXmlFilePath = FileHelpers.ExtractFileFromZip(
                submissionFilePath,
                PomXmlFileNameAndExtension,
            extractionDirectory);

            if (string.IsNullOrEmpty(pomXmlFilePath))
            {
                throw new FileNotFoundException("Pom.xml not found in submission!");
            }

            this.AddBuildSettings(pomXmlFilePath);
            this.AddDependencies(pomXmlFilePath);
            var mainClassFolderPathInZip = Path.GetDirectoryName(FileHelpers
                .GetFilePathsFromZip(submissionFilePath)
                .FirstOrDefault(f => f.EndsWith(PomXmlFileNameAndExtension)));

            FileHelpers.AddFilesToZipArchive(submissionFilePath, mainClassFolderPathInZip, pomXmlFilePath);
            DirectoryHelpers.SafeDeleteDirectory(extractionDirectory, true);
        }

        private void AddBuildSettings(string pomXmlFilePath)
        {
            var pomXmlContent = File.ReadAllText(pomXmlFilePath);
            var buildSettingsRegex = new Regex(PomXmlBuildSettingsPattern);
            if (buildSettingsRegex.IsMatch(pomXmlContent))
            {
                pomXmlContent = Regex.Replace(pomXmlContent, PomXmlBuildSettingsPattern, this.PomXmlBuildSettings);
            }

            File.WriteAllText(pomXmlFilePath, pomXmlContent);
        }

        private void AddDependencies(string pomXmlFilePath)
        {
            var doc = new XmlDocument();
            doc.Load(pomXmlFilePath);

            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("pomns", PomXmlNamespace);

            XmlNode rootNode = doc.DocumentElement;
            if (rootNode == null)
            {
                throw new XmlException("Root element not specified in pom.xml");
            }

            var dependenciesNode = rootNode.SelectSingleNode(DependenciesNodeXPath, namespaceManager);
            if (dependenciesNode == null)
            {
                throw new XmlException("No dependencies specified in pom.xml");
            }

            foreach (var groupIdArtifactId in this.Dependencies)
            {
                var dependencyNode = rootNode
                    .SelectSingleNode(
                     DependencyNodeXPathTemplate
                    .Replace("##", groupIdArtifactId.Key).Replace("!!", groupIdArtifactId.Value.Item1), namespaceManager);

                if (dependencyNode == null)
                {
                    dependencyNode = doc.CreateNode(XmlNodeType.Element, "dependency", PomXmlNamespace);

                    var groupId = doc.CreateNode(XmlNodeType.Element, "groupId", PomXmlNamespace);
                    groupId.InnerText = groupIdArtifactId.Key;
                    var artifactId = doc.CreateNode(XmlNodeType.Element, "artifactId", PomXmlNamespace);
                    artifactId.InnerText = groupIdArtifactId.Value.Item1;

                    if (groupIdArtifactId.Value.Item2 != null)
                    {
                        var versionNumber = doc.CreateNode(XmlNodeType.Element, "version", PomXmlNamespace);
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

        private string ExtractEntryPointFromPomXml(string submissionFilePath)
        {
            var pomXmlPath = FileHelpers.ExtractFileFromZip(submissionFilePath, "pom.xml", this.WorkingDirectory);

            if (string.IsNullOrEmpty(pomXmlPath))
            {
                throw new ArgumentException($"{nameof(pomXmlPath)} was not found in submission!");
            }

            var pomXml = new XmlDocument();
            pomXml.Load(pomXmlPath);

            var namespaceManager = new XmlNamespaceManager(pomXml.NameTable);
            namespaceManager.AddNamespace("pomns", PomXmlNamespace);

            XmlNode rootNode = pomXml.DocumentElement;

            var packageName = rootNode?.SelectSingleNode(StartClassNodeXPath, namespaceManager);

            if (packageName == null)
            {
                throw new ArgumentException($"Starter path not defined in pom.xml!");
            }

            FileHelpers.DeleteFiles(pomXmlPath);
            return packageName.InnerText.Trim();
        }
    }
}
