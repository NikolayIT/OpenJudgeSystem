namespace OJS.Workers.Compilers
{
    using System.Text;
    using OJS.Common;

    public class DotNetCompiler : Compiler
    {
        public override int MaxProcessExitTimeOutMillisecond => GlobalConstants.DefaultProcessExitTimeOutMilliseconds +
                                                                5000;

        public override bool ShouldDeleteSourceFile => false;

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();
            arguments.Append("build ");
            arguments.Append($"\"{inputFile}\" ");
            arguments.Append(additionalArguments);
            return arguments.ToString().Trim();
        }
    }
}
