namespace OJS.Workers.Compilers
{
    using System;

    public class MsBuildCompiler : Compiler
    {
        public override string RenameInputFile(string inputFile)
        {
            throw new NotImplementedException();
        }

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            throw new NotImplementedException();
        }

        public override void UpdateCompilerProcessStartInfo(System.Diagnostics.ProcessStartInfo processStartInfo)
        {
            throw new NotImplementedException();
        }
    }
}
