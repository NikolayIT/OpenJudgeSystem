namespace OJS.Workers.Common.Extensions
{
    using OJS.Workers.Common.Models;

    public static class CompilerTypeExtensions
    {
        public static string GetFileExtension(this CompilerType compilerType)
        {
            switch (compilerType)
            {
                case CompilerType.None:
                    return null;
                case CompilerType.CSharp:
                    return "cs";
                case CompilerType.MsBuild:
                    return "zip";
                case CompilerType.CPlusPlusGcc:
                    return "cpp";
                case CompilerType.Java:
                    return "java";
                default:
                    return null;
            }
        }
    }
}