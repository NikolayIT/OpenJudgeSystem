namespace OJS.Common.Extensions
{
    using System;

    using OJS.Common.Models;

    public static class PlagiarismDetectorTypeExtensions
    {
        public static CompilerType[] GetCompatibleCompilerTypes(this PlagiarismDetectorType plagiarismDetectorType)
        {
            switch (plagiarismDetectorType)
            {
                case PlagiarismDetectorType.CSharpCompileDisassemble:
                    return new[] { CompilerType.CSharp };
                case PlagiarismDetectorType.JavaCompileDisassemble:
                    return new[] { CompilerType.Java };
                case PlagiarismDetectorType.PlainText:
                    return new[]
                    {
                        CompilerType.None,
                        CompilerType.CSharp,
                        CompilerType.CPlusPlusGcc,
                        CompilerType.Java,
                    };
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(plagiarismDetectorType),
                        plagiarismDetectorType,
                        null);
            }
        }
    }
}
