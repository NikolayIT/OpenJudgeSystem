namespace OJS.Workers.Compilers
{
    public class CompilerOutput
    {
        public CompilerOutput(int exitCode, string output)
        {
            this.ExitCode = exitCode;
            this.Output = output;
        }

        public int ExitCode { get; set; }

        public string Output { get; set; }

        public bool IsSuccessful => this.ExitCode == 0;
    }
}
