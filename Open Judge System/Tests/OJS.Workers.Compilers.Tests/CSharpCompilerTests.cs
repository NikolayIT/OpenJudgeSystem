namespace OJS.Workers.Compilers.Tests
{
    using NUnit.Framework;

    using OJS.Workers.Common.Helpers;

    [TestFixture]
    public class CSharpCompilerTests
    {
        private const string CSharpCompilerPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe";
        private const int CSharpCompilerProcessExitTimeOutMultiplier = 1;

        [Test]
        public void CSharpCompilerShouldWorkWhenGivenValidSourceCode()
        {
            const string Source = @"using System;
class Program
{
    static void Main()
    {
        Console.WriteLine(""It works!"");
    }
}";

            var compiler = new CSharpCompiler(CSharpCompilerProcessExitTimeOutMultiplier);
            var result = compiler.Compile(CSharpCompilerPath, FileHelpers.SaveStringToTempFile(Source), string.Empty);

            Assert.IsTrue(result.IsCompiledSuccessfully, "Compilation is not successful");
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.OutputFile), "Output file is null");
            Assert.IsTrue(result.OutputFile.EndsWith(".exe"), "Output file does not ends with .exe");
        }
    }
}
