namespace OJS.Workers.Common
{
    public class CompileResult
    {
        public CompileResult(string outputFile)
        {
            this.IsCompiledSuccessfully = true;
            this.OutputFile = outputFile;
        }

        public CompileResult(bool isCompiledSuccessfully, string compilerComment)
        {
            this.IsCompiledSuccessfully = isCompiledSuccessfully;
            this.CompilerComment = compilerComment;
        }

        public bool IsCompiledSuccessfully { get; set; }

        public string CompilerComment { get; set; }

        public string OutputFile { get; set; }
    }
}
