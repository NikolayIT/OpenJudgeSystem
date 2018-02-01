namespace OJS.Workers.Compilers
{
    using System.IO;
    using System.Linq;
    using System.Text;

    public class CSharpDotNetCoreCompiler : Compiler
    {
        private const string CSharpDotNetCoreCompilerPath = @"C:\Program Files\dotnet\sdk\2.1.4\Roslyn\bincore\csc.dll";
        private const string SharedAssembliesFolderPath = @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.0.0";

        public override string GetOutputFileName(string inputFileName) => inputFileName + ".dll";

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var arguments = new StringBuilder();

            // Use dotnet.exe to run the dotnet csc.dll
            arguments.Append($"\"{CSharpDotNetCoreCompilerPath}\" ");

            // Give it all System references, since it does not do that implicitly like the old csc.exe
            var references = Directory.GetFiles(SharedAssembliesFolderPath).Where(f => f.Contains("System"));

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