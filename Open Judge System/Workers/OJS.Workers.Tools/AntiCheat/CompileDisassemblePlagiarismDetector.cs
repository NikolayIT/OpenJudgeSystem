namespace OJS.Workers.Tools.AntiCheat
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Tools.AntiCheat.Contracts;
    using OJS.Workers.Tools.Disassemblers;
    using OJS.Workers.Tools.Disassemblers.Contracts;
    using OJS.Workers.Tools.Similarity;

    public abstract class CompileDisassemblePlagiarismDetector : IPlagiarismDetector
    {
        private readonly ISimilarityFinder similarityFinder;
        private readonly IDictionary<string, string> sourcesCache;

        protected CompileDisassemblePlagiarismDetector(
            ICompiler compiler,
            string compilerPath,
            IDisassembler disassembler,
            ISimilarityFinder similarityFinder)
        {
            this.Compiler = compiler;
            this.CompilerPath = compilerPath;
            this.Disassembler = disassembler;
            this.similarityFinder = similarityFinder;
            this.sourcesCache = new Dictionary<string, string>();
        }

        protected ICompiler Compiler { get; }

        protected string CompilerPath { get; }

        protected IDisassembler Disassembler { get; }

        protected virtual string CompilerAdditionalArguments => null;

        protected virtual string DisassemblerAdditionalArguments => null;

        public PlagiarismResult DetectPlagiarism(
            string firstSource,
            string secondSource,
            IEnumerable<IDetectPlagiarismVisitor> visitors = null)
        {
            string firstDisassembledCode;
            if (!this.GetDisassembledCode(firstSource, out firstDisassembledCode))
            {
                return new PlagiarismResult(0);
            }

            string secondDisassembledCode;
            if (!this.GetDisassembledCode(secondSource, out secondDisassembledCode))
            {
                return new PlagiarismResult(0);
            }

            if (visitors != null)
            {
                foreach (var visitor in visitors)
                {
                    firstDisassembledCode = visitor.Visit(firstDisassembledCode);
                    secondDisassembledCode = visitor.Visit(secondDisassembledCode);
                }
            }

            var differences = this.similarityFinder
                .DiffText(firstDisassembledCode, secondDisassembledCode, true, true, true);

            var differencesCount = differences.Sum(difference => difference.DeletedA + difference.InsertedB);
            var textLength = firstDisassembledCode.Length + secondDisassembledCode.Length;

            // TODO: Revert the percentage
            var percentage = ((decimal)differencesCount * 100) / textLength;

            return new PlagiarismResult(percentage)
            {
                Differences = differences,
                FirstToCompare = firstDisassembledCode,
                SecondToCompare = secondDisassembledCode
            };
        }

        protected virtual bool GetDisassembledCode(string source, out string disassembledCode)
        {
            if (this.sourcesCache.ContainsKey(source))
            {
                disassembledCode = this.sourcesCache[source];
                return true;
            }

            disassembledCode = null;

            var compileResult = this.CompileCode(source);
            if (!compileResult.IsCompiledSuccessfully)
            {
                return false;
            }

            var disassembleResult = this.DisassembleFile(compileResult.OutputFile);
            if (!disassembleResult.IsDisassembledSuccessfully)
            {
                return false;
            }

            disassembledCode = disassembleResult.DisassembledCode;

            this.sourcesCache.Add(source, disassembledCode);

            return true;
        }

        protected virtual CompileResult CompileCode(string sourceCode)
        {
            var sourceFilePath = FileHelpers.SaveStringToTempFile(sourceCode);

            var compileResult = this.Compiler.Compile(
                this.CompilerPath,
                sourceFilePath,
                this.CompilerAdditionalArguments);

            if (File.Exists(sourceFilePath))
            {
                File.Delete(sourceFilePath);
            }

            return compileResult;
        }

        protected virtual DisassembleResult DisassembleFile(string compiledFilePath)
        {
            var disassemblerResult = this.Disassembler.Disassemble(
                compiledFilePath,
                this.DisassemblerAdditionalArguments);

            if (File.Exists(compiledFilePath))
            {
                File.Delete(compiledFilePath);
            }

            return disassemblerResult;
        }
    }
}
