namespace OJS.Workers.Common
{
    public class CompileResult
    {
        public CompileResult(string outputFile)
            : this(true, null, outputFile)
        {
        }

        public CompileResult(bool isCompiledSuccessfully, string compilerComment, string outputFile = null)
        {
            this.IsCompiledSuccessfully = isCompiledSuccessfully;
            this.CompilerComment = compilerComment;
            this.OutputFile = outputFile;
        }

        public bool IsCompiledSuccessfully { get; set; }

        public string CompilerComment { get; set; }

        public string OutputFile { get; set; }
    }
}
