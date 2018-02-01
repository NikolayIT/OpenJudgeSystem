namespace OJS.Workers.Compilers
{
    using System.Text;

    public class CSharpDotNetCoreCompiler : Compiler
    {
        private const string DotNetCoreFileExtension = ".dll";
        private const string CSharpDotNetCoreCompilerPath = @"C:\Program Files\dotnet\sdk\2.1.4\Roslyn\bincore\csc.dll";
        private const string SharedAssembliesFolderPath = @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\2.0.0";

        public override string GetOutputFileName(string inputFileName) =>
            inputFileName + DotNetCoreFileExtension;

        public override string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments)
        {
            var referenceLibrarieNames = new[]
            {
                "System",
                "System.Console",
                "System.Private.CoreLib",
                "System.Runtime",
                "System.Linq"
            };

            var arguments = new StringBuilder();

            // Use dotnet.exe to run csc.dll
            arguments.Append($"\"{CSharpDotNetCoreCompilerPath}\"");
            arguments.Append(' ');

            // References
            foreach (var fileName in referenceLibrarieNames)
            {
                arguments.Append($"-r:\"{SharedAssembliesFolderPath}\\{fileName}{DotNetCoreFileExtension}\"");
                arguments.Append(' ');
            }

            // Output file argument
            arguments.Append($"/out:\"{outputFile}\"");
            arguments.Append(' ');

            // Input file argument
            arguments.Append($"\"{inputFile}\"");
            arguments.Append(' ');

            arguments.Append(additionalArguments);

            return arguments.ToString().Trim();
        }
    }
}