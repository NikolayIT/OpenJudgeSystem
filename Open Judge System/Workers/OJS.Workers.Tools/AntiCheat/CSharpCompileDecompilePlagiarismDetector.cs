namespace OJS.Workers.Tools.AntiCheat
{
    using System.IO;
    using System.Linq;

    using OJS.Common.Extensions;
    using OJS.Workers.Compilers;
    using OJS.Workers.Tools.Similarity;

    public class CSharpCompileDecompilePlagiarismDetector : IPlagiarismDetector
    {
        private readonly string csharpCompilerPath;

        private readonly string dotNetDisassemblerPath;

        private readonly CSharpCompiler csharpCompiler;

        private readonly DotNetDisassembler dotNetDisassembler;

        public CSharpCompileDecompilePlagiarismDetector(string csharpCompilerPath, string dotNetDisassemblerPath)
        {
            this.csharpCompilerPath = csharpCompilerPath;
            this.dotNetDisassemblerPath = dotNetDisassemblerPath;
            this.csharpCompiler = new CSharpCompiler();
            this.dotNetDisassembler = new DotNetDisassembler();
        }

        public PlagiarismResult DetectPlagiarism(string firstSource, string secondSource)
        {
            string firstFileContent;
            if (!this.GetCilCode(firstSource, out firstFileContent))
            {
                return new PlagiarismResult(0);
            }

            string secondFileContent;
            if (!this.GetCilCode(secondSource, out secondFileContent))
            {
                return new PlagiarismResult(0);
            }

            var similarityFinder = new SimilarityFinder();
            var differences = similarityFinder.DiffText(firstFileContent, secondFileContent, true, true, true);

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

        private bool GetCilCode(string originalSource, out string fileContent)
        {
            // TODO: Check for undeleted temporary files.
            fileContent = null;

            var sourceFilePath = FileHelpers.SaveStringToTempFile(originalSource);
            var compileResult = this.csharpCompiler.Compile(this.csharpCompilerPath, sourceFilePath, null);
            File.Delete(sourceFilePath);
            if (!compileResult.IsCompiledSuccessfully)
            {
                return false;
            }

            var disassemblerResult = this.dotNetDisassembler.Compile(
                this.dotNetDisassemblerPath,
                compileResult.OutputFile,
                null);
            File.Delete(compileResult.OutputFile);
            if (!disassemblerResult.IsCompiledSuccessfully)
            {
                return false;
            }

            fileContent = File.ReadAllText(disassemblerResult.OutputFile);
            File.Delete(disassemblerResult.OutputFile);
            return true;
        }
    }
}
