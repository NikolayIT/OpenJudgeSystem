namespace OJS.Workers.Compilers
{
    using System;
    using System.Diagnostics;

    public class JavaScriptFilePreprocessor : Compiler
    {
        public override string RenameInputFile(string inputFile)
        {
            throw new NotImplementedException();
        }

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            throw new NotImplementedException();
        }

        public override void UpdateCompilerProcessStartInfo(ProcessStartInfo processStartInfo)
        {
            throw new NotImplementedException();
        }
    }
}
