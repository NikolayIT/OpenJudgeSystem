namespace OJS.Workers.Tools.AntiCheat
{
    using OJS.Common.Extensions;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Tools.Disassemblers.Contracts;
    using OJS.Workers.Tools.Similarity;

    public class JavaCompileDisassemblePlagiarismDetector : CompileDisassemblePlagiarismDetector
    {
        // http://docs.oracle.com/javase/8/docs/technotes/tools/windows/javap.html
        private const string JavaDisassemblerShowAllClassesAndMembersArgument = "-p";

        private readonly string workingDirectory;

        public JavaCompileDisassemblePlagiarismDetector(
            ICompiler compiler,
            string compilerPath,
            IDisassembler disassembler,
            ISimilarityFinder similarityFinder)
            : base(compiler, compilerPath, disassembler, similarityFinder)
        {
            this.workingDirectory = DirectoryHelpers.CreateTempDirectory();
        }

        ~JavaCompileDisassemblePlagiarismDetector()
        {
            DirectoryHelpers.SafeDeleteDirectory(this.workingDirectory, true);
        }

        protected override CompileResult CompileCode(string sourceCode)
        {
            var sourceFilePath = JavaCodePreprocessorHelper.CreateSubmissionFile(sourceCode, this.workingDirectory);

            var compileResult = this.Compiler.Compile(
                this.CompilerPath,
                sourceFilePath,
                this.GetCompilerAdditionalArguments());

            return compileResult;
        }

        protected override string GetDisassemblerAdditionalArguments() =>
            JavaDisassemblerShowAllClassesAndMembersArgument;
    }
}
