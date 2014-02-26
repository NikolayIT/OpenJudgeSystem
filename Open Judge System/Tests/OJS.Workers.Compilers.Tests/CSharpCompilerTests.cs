namespace OJS.Workers.Compilers.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using OJS.Common.Extensions;

    [TestClass]
    public class CSharpCompilerTests
    {
        private const string CSharpCompilerPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe";

        [TestMethod]
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

            var compiler = new CSharpCompiler();
            var result = compiler.Compile(CSharpCompilerPath, FileHelpers.SaveStringToTempFile(Source), string.Empty);

            Assert.IsTrue(result.IsCompiledSuccessfully);
            Assert.IsTrue(string.IsNullOrWhiteSpace(result.OutputFile));
            Assert.IsTrue(result.OutputFile.EndsWith(".exe"));
        }
    }
}
