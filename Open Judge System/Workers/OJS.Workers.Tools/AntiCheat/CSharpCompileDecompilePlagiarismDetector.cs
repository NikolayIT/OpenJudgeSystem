namespace OJS.Workers.Tools.AntiCheat
{
    using System.Collections.Generic;
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

        private readonly ISimilarityFinder similarityFinder;

        private readonly IDictionary<string, string> sourcesCache;

        public CSharpCompileDecompilePlagiarismDetector(string csharpCompilerPath, string dotNetDisassemblerPath)
            : this(csharpCompilerPath, dotNetDisassemblerPath, new SimilarityFinder())
        {
        }

        public CSharpCompileDecompilePlagiarismDetector(string csharpCompilerPath, string dotNetDisassemblerPath, ISimilarityFinder similarityFinder)
        {
            this.csharpCompilerPath = csharpCompilerPath;
            this.dotNetDisassemblerPath = dotNetDisassemblerPath;
            this.csharpCompiler = new CSharpCompiler();
            this.dotNetDisassembler = new DotNetDisassembler();
            this.similarityFinder = similarityFinder;
            this.sourcesCache = new Dictionary<string, string>();
        }

        public PlagiarismResult DetectPlagiarism(string firstSource, string secondSource, IEnumerable<IDetectPlagiarismVisitor> visitors = null)
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

            if (visitors != null)
            {
                foreach (var visitor in visitors)
                {
                    firstFileContent = visitor.Visit(firstFileContent);
                    secondFileContent = visitor.Visit(secondFileContent);
                }
            }

            var differences = this.similarityFinder.DiffText(firstFileContent, secondFileContent, true, true, true);

            var differencesCount = differences.Sum(difference => difference.DeletedA + difference.InsertedB);
            var textLength = firstFileContent.Length + secondFileContent.Length;

            // TODO: Revert the percentage
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
            if (this.sourcesCache.ContainsKey(originalSource))
            {
                fileContent = this.sourcesCache[originalSource];
                return true;
            }

            // TODO: Check for undeleted temporary files.
            fileContent = null;

            var sourceFilePath = FileHelpers.SaveStringToTempFile(originalSource);
            var compileResult = this.csharpCompiler.Compile(this.csharpCompilerPath, sourceFilePath, "/optimize+ /nologo /reference:System.Numerics.dll /reference:PowerCollections.dll");
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
            this.sourcesCache.Add(originalSource, fileContent);
            File.Delete(disassemblerResult.OutputFile);
            return true;
        }
    }
}
