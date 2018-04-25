namespace OJS.Workers.ExecutionStrategies.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using Microsoft.Build.Evaluation;

    internal static class CSharpProjectExtensions
    {
        private const string MicrosoftCsProjXmlNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        private const string NuGetXmlNodeXPath = @"//msns:Target[@Name='EnsureNuGetPackageBuildImports']";
        private const string VsToolsXmlNodeXPath = @"//msns:Import[@Project='$(VSToolsPath)\WebApplications\Microsoft.WebApplication.targets']";

        public static void RemoveNuGetPackageImportsTarget(this Project project)
        {
            if (project.Targets.ContainsKey("EnsureNuGetPackageBuildImports"))
            {
                RemoveXmlNodeFromCsProj(project.FullPath, NuGetXmlNodeXPath);
            }
        }

        public static void RemoveVsToolsImport(this Project project)
        {
            var vsToolsImport = project.Imports.Any(i =>
                i.ImportingElement.Project ==
                    "$(VSToolsPath)\\WebApplications\\Microsoft.WebApplication.targets");

            if (vsToolsImport)
            {
                RemoveXmlNodeFromCsProj(project.FullPath, VsToolsXmlNodeXPath);
            }
        }

        public static void RemoveItemByName(this Project project, string itemName)
        {
            var item = project.Items.FirstOrDefault(pi =>
                pi.EvaluatedInclude.Contains(itemName));

            if (item != null)
            {
                project.RemoveItem(item);
            }
        }

        public static void AddReferences(this Project project, params string[] references)
        {
            RemoveExistingReferences(project, references);

            foreach (var reference in references)
            {
                var referenceMetaData = new Dictionary<string, string>
                {
                    { "SpecificVersion", "False" },
                    { "Private", "True" }
                };

                project.AddItem("Reference", reference, referenceMetaData);
            }
        }

        public static void EnsureAssemblyNameIsCorrect(this Project project)
        {
            var assemblyNameProperty = project.AllEvaluatedProperties.FirstOrDefault(x => x.Name == "AssemblyName");
            if (assemblyNameProperty == null)
            {
                throw new ArgumentException("Project file does not contain Assembly Name property!");
            }

            var csProjFullpath = project.FullPath;
            var projectName = Path.GetFileNameWithoutExtension(csProjFullpath);
            project.SetProperty("AssemblyName", projectName);
        }

        private static void RemoveExistingReferences(Project project, IEnumerable<string> references)
        {
            foreach (var reference in references)
            {
                var referenceName = reference.Substring(0, reference.IndexOf(","));
                var existingReference = project.Items.FirstOrDefault(x => x.EvaluatedInclude.Contains(referenceName));
                if (existingReference != null)
                {
                    project.RemoveItem(existingReference);
                }
            }
        }

        private static void RemoveXmlNodeFromCsProj(string csprojPath, string xpathExpression)
        {
            var csprojXml = new XmlDocument();
            csprojXml.Load(csprojPath);
            var namespaceManager = new XmlNamespaceManager(csprojXml.NameTable);
            namespaceManager.AddNamespace("msns", MicrosoftCsProjXmlNamespace);

            XmlNode rootNode = csprojXml.DocumentElement;
            var targetNode = rootNode.SelectSingleNode(xpathExpression, namespaceManager);
            rootNode.RemoveChild(targetNode);
            csprojXml.Save(csprojPath);
        }
    }
}