namespace OJS.Workers.Common.Helpers
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    public static class JavaCodePreprocessorHelper
    {
        private const string PackageNameRegEx = @"\bpackage\s+[a-zA-Z_][a-zA-Z_.0-9]{0,150}\s*;";
        private const string ClassNameRegEx = @"public\s+class\s+([a-zA-Z_][a-zA-Z_0-9]{0,150})\s*{";
        private const int ClassNameRegExGroup = 1;

        public static string CreateSubmissionFile(string sourceCode, string directory)
        {
            // Remove existing packages
            sourceCode = Regex.Replace(sourceCode, PackageNameRegEx, string.Empty);

            // TODO: Remove the restriction for one public class - a non-public Java class can contain the main method!
            var classNameMatch = Regex.Match(sourceCode, ClassNameRegEx);
            if (!classNameMatch.Success)
            {
                throw new ArgumentException("No valid public class found!");
            }

            var className = classNameMatch.Groups[ClassNameRegExGroup].Value;
            var submissionFilePath = $"{directory}\\{className}";

            File.WriteAllText(submissionFilePath, sourceCode);

            return submissionFilePath;
        }
    }
}