namespace OJS.Workers.Compilers
{
    using System;

    public class SolidityCompiler : Compiler
    {
        public SolidityCompiler(int processExitTimeOutMultiplier)
            : base(processExitTimeOutMultiplier)
        {
        }

        public override string BuildCompilerArguments(
            string inputFile,
            string outputFile,
            string additionalArguments)
        {
            throw new NotImplementedException();
        }
    }
}