namespace OJS.Workers.Compilers.Tests
{
    using NUnit.Framework;

    using OJS.Common.Extensions;

    [TestFixture]
    public class CSharpCompilerTests
    {
        private const string CSharpCompilerPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe";

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

            var compiler = new CSharpCompiler();
            var result = compiler.Compile(CSharpCompilerPath, FileHelpers.SaveStringToTempFile(Source), string.Empty);

            Assert.IsTrue(result.IsCompiledSuccessfully);
            Assert.IsNotNullOrEmpty(result.OutputFile);
            Assert.IsTrue(result.OutputFile.EndsWith(".exe"));
        }
    }
}
