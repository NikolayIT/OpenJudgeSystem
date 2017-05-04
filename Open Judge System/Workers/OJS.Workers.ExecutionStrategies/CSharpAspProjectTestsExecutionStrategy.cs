namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    using Microsoft.Build.Evaluation;

    using OJS.Common.Models;

    public class CSharpAspProjectTestsExecutionStrategy : CSharpProjectTestsExecutionStrategy
    {
        protected const string MoqAssemblyReference =
            "Moq, Version=4.7.8.0, Culture=neutral, PublicKeyToken=69f491c39445e920, processorArchitecture=MSIL";
        protected const string CastleCoreAssemblyReference =
            @"Castle.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=407dd0808d44fbdc";
        protected const string NuGetXmlNodeXPath = @"//msns:Target[@Name='EnsureNuGetPackageBuildImports']";
        protected const string MicrosoftCsProjXmlNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        public CSharpAspProjectTestsExecutionStrategy(
            string nUnitConsoleRunnerPath,
            Func<CompilerType, string> getCompilerPathFunc)
            : base(nUnitConsoleRunnerPath, getCompilerPathFunc)
        {
        }

        protected override void CorrectProjectReferences(IEnumerable<TestContext> tests, Project project)
        {
            this.AddProjectReferences(project, MoqAssemblyReference, CastleCoreAssemblyReference);

            base.CorrectProjectReferences(tests, project);

            bool nuGetPackageImportsTarget = project.Targets.ContainsKey("EnsureNuGetPackageBuildImports");
            if (nuGetPackageImportsTarget)
            {
                this.RemoveNuGetTarget(project.FullPath);
            }
        }

        private void RemoveNuGetTarget(string projectFullPath)
        {
            XmlDocument csprojXml = new XmlDocument();
            csprojXml.Load(projectFullPath);
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(csprojXml.NameTable);
            namespaceManager.AddNamespace("msns", MicrosoftCsProjXmlNamespace);

            XmlNode rootNode = csprojXml.DocumentElement;
            var nuGetTargetNode = rootNode.SelectSingleNode(NuGetXmlNodeXPath, namespaceManager);
            rootNode.RemoveChild(nuGetTargetNode);
            csprojXml.Save(projectFullPath);
        }
    }
}
