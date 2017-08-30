namespace OJS.Workers.ExecutionStrategies
{
    public class DotNetCoreProjectTestsExecutionStrategy
    {
        // This code will be used in the next testing strategy. DO NOT DELETE!

        // protected const string GenerateProgramFileNodeName = @"GenerateProgramFile";
        // protected const string PropertyGroupNodeName = @"PropertyGroup";
        // protected const string PackageReferenceNodeName = @"PackageReference";

        // protected const string ItemGroupNodeXPath = @"ItemGroup";
        // protected const string GenerateProgramFileXPath = @"PropertyGroup/GenerateProgramFile[.='false' or .='False']";
        // protected const string PackageReferenceXPathExpressionTemplate =
        //     @"ItemGroup/PackageReference[@Include='##packageName##']";

        // // TODO: update to newer versions
        // protected static readonly Dictionary<string, string> RequiredReferencedAndVersions = new Dictionary<string, string>
        // {
        //     { "Microsoft.NET.Test.Sdk", "15.5.0-preview-20170727-01"},
        //     { "NUnit", "3.7.1" },
        //     { "NUnit3TestAdapter", "3.8" }
        // };

        // private void PrepareCsProjFile(string csProjFilePath)
        // {
        //     XmlDocument document = new XmlDocument();
        //     document.Load(csProjFilePath);
           
        //     this.AddPropertyGroupToCsProj(document, GenerateProgramFileXPath, csProjFilePath);
           
        //     foreach (var requiredReferencedAndVersion in RequiredReferencedAndVersions)
        //     {
        //         var packageName = requiredReferencedAndVersion.Key;
        //         var packageVersion = requiredReferencedAndVersion.Value;
        //         var xpathExpression =
        //             PackageReferenceXPathExpressionTemplate.Replace("##packageName##", packageName);
           
        //         this.AddPackageReferenceToCsProj(
        //             document,
        //             xpathExpression,
        //             csProjFilePath,
        //             new[] { packageName, packageVersion });
        //     }
           
        // }
           
        // private void AddPropertyGroupToCsProj(
        //     XmlDocument document,
        //     string xpathExpression,
        //     string savePath)
        // {
        //     XmlNode rootNode = document.DocumentElement;
        //     if (this.NodeExists(rootNode, xpathExpression))
        //     {
        //         return;
        //     }
           
        //     XmlNode generateProgramFileNode = document.CreateNode(XmlNodeType.Element, GenerateProgramFileNodeName, string.Empty);
        //     XmlNode propertyGroupNode = document.CreateNode(XmlNodeType.Element, PropertyGroupNodeName, string.Empty);
           
        //     generateProgramFileNode.InnerText = "false";
        //     propertyGroupNode.AppendChild(generateProgramFileNode);
        //     rootNode.AppendChild(propertyGroupNode);
        //     document.Save(savePath);
        // }
           
        // private void AddPackageReferenceToCsProj(XmlDocument document, string xpathExpression, string savePath, params string[] nodeValues)
        // {
        //     XmlNode rootNode = document.DocumentElement;
        //     if (this.NodeExists(rootNode, xpathExpression))
        //     {
        //         return;
        //     }
           
        //     XmlNode packageReference = document.CreateNode(XmlNodeType.Element, PackageReferenceNodeName, string.Empty);
        //     XmlAttribute includeAttribute = document.CreateAttribute("Include");
        //     XmlAttribute versionAttribute = document.CreateAttribute("Version");
        //     includeAttribute.InnerText = nodeValues[0];
        //     versionAttribute.InnerText = nodeValues[1];
           
        //     packageReference.Attributes.Append(includeAttribute);
        //     packageReference.Attributes.Append(versionAttribute);
           
        //     var itemGroupNode = rootNode.SelectSingleNode(ItemGroupNodeXPath);
        //     itemGroupNode.AppendChild(packageReference);
        //     document.Save(savePath);
        // }
           
        // private bool NodeExists(XmlNode rootNode, string xpathExpression)
        // {
        //     if (rootNode == null)
        //     {
        //         throw new ArgumentException("Project file is malformed");
        //     }
           
        //     var targetNode = rootNode.SelectSingleNode(xpathExpression);
           
        //     if (targetNode != null)
        //     {
        //         return true;
        //     }
           
        //     return false;
        // }
    }
}
