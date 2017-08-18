namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml;
    using Checkers;
    using Common;
    using Executors;
    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;

    public class DotNetCoreProjectExecutionStrategy : CSharpProjectTestsExecutionStrategy
    {
        protected const string GenerateProgramFileNodeName = @"GenerateProgramFile";
        protected const string PropertyGroupNodeName = @"PropertyGroup";
        protected const string PackageReferenceNodeName = @"PackageReference";

        protected const string ItemGroupNodeXPath = @"ItemGroup";
        protected const string GenerateProgramFileXPath = @"PropertyGroup/GenerateProgramFile[.='false' or .='False']";
        protected const string PackageReferenceXPathExpressionTemplate =
            @"ItemGroup/PackageReference[@Include='##packageName##']";

        protected const string SuccessfulRestorationPattern = @"Restore completed in \d+[,.]*\d+ sec for (.+)##csProjFileName##\.csproj";

        // TODO: update to newer versions
        protected static readonly Dictionary<string, string> RequiredReferencedAndVersions = new Dictionary<string, string>
        {
            { "Microsoft.NET.Test.Sdk", "15.5.0-preview-20170727-01"},
            { "NUnit", "3.7.1" },
            { "NUnit3TestAdapter", "3.8.0-alpha1" }
        };

        public DotNetCoreProjectExecutionStrategy(Func<CompilerType, string> getCompilerPathFunc)
            : base(getCompilerPathFunc)
        {
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            var result = new ExecutionResult();

            var userSubmissionContent = executionContext.FileContent;

            var submissionFilePath = $"{this.WorkingDirectory}\\{ZippedSubmissionName}";
            File.WriteAllBytes(submissionFilePath, userSubmissionContent);
            FileHelpers.RemoveFilesFromZip(submissionFilePath, RemoveMacFolderPattern);
            FileHelpers.UnzipFile(submissionFilePath, this.WorkingDirectory);

            File.Delete(submissionFilePath);

            var csProjFilePath = FileHelpers.FindFileMatchingPattern(
                this.WorkingDirectory,
                CsProjFileSearchPattern,
                f => new FileInfo(f).Length);

            this.ExtractTestNames(executionContext.Tests);
            var compileDirectory = Path.GetDirectoryName(csProjFilePath);
            this.SetupFixturePath = $"{compileDirectory}\\{SetupFixtureFileName}{GlobalConstants.CSharpFileExtension}";

            this.PrepareCsProjFile(csProjFilePath);

            var compilerPath = this.GetCompilerPathFunc(executionContext.CompilerType);

            List<string> arguments = new List<string>();
            arguments.Add("restore");
            arguments.Add(csProjFilePath);

            var restoreResult = this.RunDotNetRestoreOnProject(
                compilerPath,
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                arguments);

            var restoreMatcher = new Regex(SuccessfulRestorationPattern.Replace(
                "##csProjFileName##",
                Path.GetFileNameWithoutExtension(csProjFilePath)));

            if (restoreResult.Type != ProcessExecutionResultType.Success || 
                !restoreMatcher.IsMatch(restoreResult.ReceivedOutput))
            {
                result.IsCompiledSuccessfully = false;
                return result;
            }

            //var expendableCsProjFile = $"{Path.GetDirectoryName(csProjFilePath)}\\{Path.GetFileNameWithoutExtension(csProjFilePath)}Copy.csproj";
            //File.Copy(csProjFilePath, expendableCsProjFile);

            var compilerResult = this.Compile(
                executionContext.CompilerType,
                compilerPath,
                executionContext.AdditionalCompilerArguments,
                csProjFilePath);

            if (!compilerResult.IsCompiledSuccessfully)
            {
                result.IsCompiledSuccessfully = false;
                return result;
            }

            var executor = new RestrictedProcessExecutor();
            var checker = Checker.CreateChecker(
                executionContext.CheckerAssemblyName,
                executionContext.CheckerTypeName,
                executionContext.CheckerParameter);

            result = this.RunUnitTests(executionContext, executor, checker, result, csProjFilePath);

            return result;
        }

        protected override ExecutionResult RunUnitTests(
            ExecutionContext executionContext,
            IExecutor executor, 
            IChecker checker,
            ExecutionResult result,
            string compiledFile)
        {
            List<string> arguments = new List<string>();
            arguments.Add("test");
            arguments.Add(compiledFile);
            arguments.Add("--no-build");
            arguments.Add("--no-restore");
            arguments.Add(@"-s C:\Windows\Temp\setting.runsettings");         

            var dotNetCli = this.GetCompilerPathFunc(executionContext.CompilerType);

            var executionResult = executor.Execute(
                dotNetCli,
                string.Empty,
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                arguments,
                this.WorkingDirectory);                  

            return null;
        }
         
        private ProcessExecutionResult RunDotNetRestoreOnProject(
            string dotNetCli,
            int timeLimit,
            int memoryLimit,
            IEnumerable<string> arguments)
        {
            var processExecutor = new StandardProcessExecutor();
            var restoreResult = processExecutor.Execute(dotNetCli, string.Empty, timeLimit * 2, memoryLimit * 2, arguments);
            return restoreResult;
        }

        private void PrepareCsProjFile(string csProjFilePath)
        {
            XmlDocument document = new XmlDocument();
            document.Load(csProjFilePath);

            this.AddPropertyGroupToCsProj(document, GenerateProgramFileXPath, csProjFilePath);

            foreach (var requiredReferencedAndVersion in RequiredReferencedAndVersions)
            {
                var packageName = requiredReferencedAndVersion.Key;
                var packageVersion = requiredReferencedAndVersion.Value;
                var xpathExpression =
                    PackageReferenceXPathExpressionTemplate.Replace("##packageName##", packageName);

                this.AddPackageReferenceToCsProj(
                    document,
                    xpathExpression,
                    csProjFilePath,
                    new[] { packageName, packageVersion });
            }

        }

        private void AddPropertyGroupToCsProj(
            XmlDocument document,
            string xpathExpression,
            string savePath)
        {
            XmlNode rootNode = document.DocumentElement;
            if (this.NodeExists(rootNode, xpathExpression))
            {
                return;
            }

            XmlNode generateProgramFileNode = document.CreateNode(XmlNodeType.Element, GenerateProgramFileNodeName, string.Empty);
            XmlNode propertyGroupNode = document.CreateNode(XmlNodeType.Element, PropertyGroupNodeName, string.Empty);

            generateProgramFileNode.InnerText = "false";
            propertyGroupNode.AppendChild(generateProgramFileNode);
            rootNode.AppendChild(propertyGroupNode);
            document.Save(savePath);
        }

        private void AddPackageReferenceToCsProj(XmlDocument document, string xpathExpression, string savePath, params string[] nodeValues)
        {
            XmlNode rootNode = document.DocumentElement;
            if (this.NodeExists(rootNode, xpathExpression))
            {
                return;
            }

            XmlNode packageReference = document.CreateNode(XmlNodeType.Element, PackageReferenceNodeName, string.Empty);
            XmlAttribute includeAttribute = document.CreateAttribute("Include");
            XmlAttribute versionAttribute = document.CreateAttribute("Version");
            includeAttribute.InnerText = nodeValues[0];
            versionAttribute.InnerText = nodeValues[1];

            packageReference.Attributes.Append(includeAttribute);
            packageReference.Attributes.Append(versionAttribute);

            var itemGroupNode = rootNode.SelectSingleNode(ItemGroupNodeXPath);
            itemGroupNode.AppendChild(packageReference);
            document.Save(savePath);
        }

        private bool NodeExists(XmlNode rootNode, string xpathExpression)
        {
            if (rootNode == null)
            {
                throw new ArgumentException("Project file is malformed");
            }

            var targetNode = rootNode.SelectSingleNode(xpathExpression);

            if (targetNode != null)
            {
                return true;
            }

            return false;
        }
    }
}
