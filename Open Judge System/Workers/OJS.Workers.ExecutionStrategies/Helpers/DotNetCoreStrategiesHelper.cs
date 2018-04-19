namespace OJS.Workers.ExecutionStrategies.Helpers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal static class DotNetCoreStrategiesHelper
    {
        /// <summary>
        /// Removes all "<ProjectReference />" items from the given .csproj path
        /// </summary>
        /// <param name="csProjPath">Path to .csproj file</param>
        public static void RemoveAllProjectReferencesFromCsProj(string csProjPath)
        {
            var projectReferencesSearchRegex =
                new Regex(@"(<ItemGroup>\s*<ProjectReference(?s)(.*)<\/ItemGroup>)");

            DeleteTextFromFileByPathAndRegex(csProjPath, projectReferencesSearchRegex);
        }

        /// <summary>
        /// Removes all "<PackageReference />" items from the given .csproj path
        /// that Include the given package names
        /// </summary>
        /// <param name="csProjPath">Path to .csproj file</param>
        /// <param name="packageNames">The names of the packages that should be removed (case insensitive)</param>
        public static void RemovePackageReferencesFromCsProj(string csProjPath, IEnumerable<string> packageNames)
        {
            packageNames = packageNames.Select(pn => pn.Replace(".", "\\.").Trim());

            var packageReferencesSearchRegex = new Regex(
                $@"<PackageReference\s+Include=""\s*(?:{string.Join("|", packageNames)})\s*"".+\/>",
                RegexOptions.IgnoreCase);

            DeleteTextFromFileByPathAndRegex(csProjPath, packageReferencesSearchRegex);
        }

        private static void DeleteTextFromFileByPathAndRegex(string path, Regex regex)
        {
            var text = regex.Replace(File.ReadAllText(path), string.Empty);

            File.WriteAllText(path, text);
        }
    }
}