namespace OJS.Workers.Tools.AntiCheat
{
    using System.IO;
    using System.Text;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Tools.Disassemblers;
    using OJS.Workers.Tools.Disassemblers.Contracts;
    using OJS.Workers.Tools.Similarity;

    public class JavaCompileDisassemblePlagiarismDetector : CompileDisassemblePlagiarismDetector
    {
        // http://docs.oracle.com/javase/8/docs/technotes/tools/windows/javap.html
        private const string JavaDisassemblerAllClassesAndMembersArgument = "-p";
        private const string JavaCompiledFilesSearchPattern = "*.class";

        private string workingDirectory;

        public JavaCompileDisassemblePlagiarismDetector(
            ICompiler compiler,
            string compilerPath,
            IDisassembler disassembler,
            ISimilarityFinder similarityFinder)
            : base(compiler, compilerPath, disassembler, similarityFinder)
        {
        }

        ~JavaCompileDisassemblePlagiarismDetector()
        {
            DirectoryHelpers.SafeDeleteDirectory(this.workingDirectory, true);
        }

        protected override string DisassemblerAdditionalArguments => JavaDisassemblerAllClassesAndMembersArgument;

        protected override CompileResult CompileCode(string sourceCode)
        {
            // First delete old working directory
            DirectoryHelpers.SafeDeleteDirectory(this.workingDirectory, true);

            this.workingDirectory = DirectoryHelpers.CreateTempDirectory();

            var sourceFilePath = JavaCodePreprocessorHelper.CreateSubmissionFile(sourceCode, this.workingDirectory);

            var compileResult = this.Compiler.Compile(
                this.CompilerPath,
                sourceFilePath,
                this.CompilerAdditionalArguments);

            return compileResult;
        }

        protected override DisassembleResult DisassembleFile(string compiledFilePath)
        {
            var compiledFilesToDisassemble =
                Directory.GetFiles(this.workingDirectory, JavaCompiledFilesSearchPattern, SearchOption.AllDirectories);

            var result = new DisassembleResult(compiledFilesToDisassemble.Length > 0);

            var disassembledCode = new StringBuilder();

            foreach (var compiledFile in compiledFilesToDisassemble)
            {
                var currentDisassembleResult =
                    this.Disassembler.Disassemble(compiledFile, this.DisassemblerAdditionalArguments);
                if (!currentDisassembleResult.IsDisassembledSuccessfully)
                {
                    result.IsDisassembledSuccessfully = false;
                    break;
                }

                disassembledCode.AppendLine(currentDisassembleResult.DisassembledCode);
            }

            if (result.IsDisassembledSuccessfully)
            {
                result.DisassembledCode = disassembledCode.ToString();
            }

            DirectoryHelpers.SafeDeleteDirectory(this.workingDirectory, true);

            return result;
        }
    }
}
