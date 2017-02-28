namespace OJS.Workers.Common.Helpers
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    public static class JavaCodePreprocessorHelper
    {
        private const string PackageNameRegEx = @"\bpackage\s+[a-zA-Z_][a-zA-Z_.0-9]{0,150}\s*;";
        private const string PublicClassNameRegEx = @"public\s+class\s+([a-zA-Z_][a-zA-Z_0-9]{0,150})\s*{";
        private const string ClassNameRegEx = @"class\s+([a-zA-Z_][a-zA-Z_0-9]{0,150})\s*{";
        private const int PublicClassNameRegExGroup = 1;
        private const int ClassNameRegExGroup = 1;

        public static string CreateSubmissionFile(string sourceCode, string directory)
        {
            sourceCode = Regex.Replace(sourceCode, PackageNameRegEx, string.Empty);
            var className = GetClassName(sourceCode);
            var submissionFilePath = $"{directory}\\{className}";

            File.WriteAllText(submissionFilePath, sourceCode);

            return submissionFilePath;
        }

        public static string GetClassName(string sourceCode)
        {
            var classNameMatch = Regex.Match(sourceCode, ClassNameRegEx);
            if (!classNameMatch.Success)
            {
                throw new ArgumentException("No valid class found!");
            }

            return classNameMatch.Groups[ClassNameRegExGroup].Value;
        }

        public static string GetPublicClassName(string sourceCode)
        {
            var classNameMatch = Regex.Match(sourceCode, PublicClassNameRegEx);
            if (!classNameMatch.Success)
            {
                throw new ArgumentException("No valid public class found!");
            }

            return classNameMatch.Groups[PublicClassNameRegExGroup].Value;
        }
    }
}
