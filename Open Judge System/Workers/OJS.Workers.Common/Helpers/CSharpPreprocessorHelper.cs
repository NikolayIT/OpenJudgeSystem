namespace OJS.Workers.Common.Helpers
{
    using System;
    using System.Text.RegularExpressions;

    public static class CSharpPreprocessorHelper
    {
        private const string PublicClassNameRegEx = @"public\s+class\s+([a-zA-Z_][a-zA-Z_0-9$]{0,150})\s*{";
        private const string ClassNameRegEx = @"class\s+([a-zA-Z_][a-zA-Z_0-9$]{0,150})\s*{";
        private const string NamespaceNameRegEx = @"namespace\s+([a-zA-Z_][a-zA-Z_0-9$]{0,150})\s*{";
        private const int PublicClassNameRegExGroup = 1;
        private const int ClassNameRegExGroup = 1;
        private const int NamespaceNameRegExGroup = 1;

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

        public static string GetNamespaceName(string sourceCode)
        {
            var classNameMatch = Regex.Match(sourceCode, NamespaceNameRegEx);
            if (!classNameMatch.Success)
            {
                return null;
            }

            return classNameMatch.Groups[NamespaceNameRegExGroup].Value;
        }
    }
}
