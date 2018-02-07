namespace OJS.Workers.Compilers
{
    using System.IO;
    using System.Linq;
    using System.Text;

    public class CSharpDotNetCoreCompiler : Compiler
    {
        private readonly string cSharpDotNetCoreCompilerPath;
        private readonly string dotNetCoreSharedAssembliesPath;

        public CSharpDotNetCoreCompiler(
            int processExitTimeOutMultiplier,
            string cSharpDotNetCoreCompilerPath,
            string dotNetCoreSharedAssembliesPath)
            : base(processExitTimeOutMultiplier)
        {
            this.cSharpDotNetCoreCompilerPath = cSharpDotNetCoreCompilerPath;
            this.dotNetCoreSharedAssembliesPath = dotNetCoreSharedAssembliesPath;
        }

        public override string GetOutputFileName(string inputFileName) => inputFileName + ".dll";

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();

            // Use dotnet.exe to run the csc.dll
            arguments.Append($"\"{this.cSharpDotNetCoreCompilerPath}\" ");

            // Give it all System references
            var references = Directory.GetFiles(this.dotNetCoreSharedAssembliesPath).Where(f => f.Contains("System"));

            foreach (var reference in references)
            {
                arguments.Append($"-r:\"{reference}\" ");
            }

            // Output file argument
            arguments.Append($"/out:\"{outputFile}\" ");

            // Input file argument
            arguments.Append($"\"{inputFile}\" ");

            arguments.Append(additionalArguments);

            return arguments.ToString().Trim();
        }
    }
}