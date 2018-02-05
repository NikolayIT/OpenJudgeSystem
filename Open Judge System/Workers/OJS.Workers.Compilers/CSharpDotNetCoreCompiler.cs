namespace OJS.Workers.Compilers
{
    using System.Linq;
    using System.Text;

    public class CSharpDotNetCoreCompiler : Compiler
    {
        public override string GetOutputFileName(string inputFileName) => inputFileName + ".dll";

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();

            var additionalCompilerArguments = additionalArguments.Split('|');
            var cSharpDotNetCoreCompilerPath = additionalCompilerArguments.First();
            additionalArguments = additionalCompilerArguments.Last();

            // Use dotnet.exe to run the csc.dll
            arguments.Append($"\"{cSharpDotNetCoreCompilerPath}\" ");

            // Output file argument
            arguments.Append($"/out:\"{outputFile}\" ");

            // Input file argument
            arguments.Append($"\"{inputFile}\" ");

            arguments.Append(additionalArguments);

            return arguments.ToString().Trim();
        }
    }
}