namespace OJS.Workers.Tools.AntiCheat
{
    using System.IO;
    using System.Linq;

    using OJS.Workers.Compilers;
    using OJS.Workers.Tools.Similarity;

    public class CSharpCompileDecompilePlagiarismDetector : IPlagiarismDetector
    {
        private readonly string csharpCompilerPath;

        private readonly string dotNetDisassemblerPath;

        public CSharpCompileDecompilePlagiarismDetector(string csharpCompilerPath, string dotNetDisassemblerPath)
        {
            this.csharpCompilerPath = csharpCompilerPath;
            this.dotNetDisassemblerPath = dotNetDisassemblerPath;
        }

        public PlagiarismResult DetectPlagiarism(string firstSource, string secondSource)
        {
            var csharpCompiler = new CSharpCompiler();
            var firstCompileResult = csharpCompiler.Compile(this.csharpCompilerPath, firstSource, null);
            if (!firstCompileResult.IsCompiledSuccessfully)
            {
                return new PlagiarismResult(0);
            }

            var secondCompileResult = csharpCompiler.Compile(this.csharpCompilerPath, secondSource, null);
            if (!secondCompileResult.IsCompiledSuccessfully)
            {
                return new PlagiarismResult(0);
            }

            var dotNetDisassembler = new DotNetDisassembler();
            var firstDisassemblerResult = dotNetDisassembler.Compile(this.dotNetDisassemblerPath, firstCompileResult.OutputFile, null);
            if (!firstDisassemblerResult.IsCompiledSuccessfully)
            {
                return new PlagiarismResult(0);
            }

            var secondDisassemblerResult = dotNetDisassembler.Compile(this.dotNetDisassemblerPath, firstCompileResult.OutputFile, null);
            if (!secondDisassemblerResult.IsCompiledSuccessfully)
            {
                return new PlagiarismResult(0);
            }

            var firstFileContent = File.ReadAllText(firstDisassemblerResult.OutputFile);
            var secondFileContent = File.ReadAllText(secondDisassemblerResult.OutputFile);

            var similarityFinder = new SimilarityFinder();
            var differences = similarityFinder.DiffText(
                firstFileContent,
                secondFileContent,
                true,
                true,
                true);

            var differencesCount = differences.Sum(difference => difference.DeletedA + difference.InsertedB);
            var textLength = firstFileContent.Length + secondFileContent.Length;

            var percentage = ((decimal)differencesCount * 100) / textLength;

            return new PlagiarismResult(percentage)
                       {
                           Differences = differences,
                           FirstToCompare = firstFileContent,
                           SecondToCompare = secondFileContent
                       };
        }
    }
}
