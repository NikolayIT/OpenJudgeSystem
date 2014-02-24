namespace OJS.Workers.Compilers.Tests
{
    using System.IO;

    using NUnit.Framework;

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
            var result = compiler.Compile(CSharpCompilerPath, this.SaveStringToTempFile(Source), string.Empty);

            Assert.IsTrue(result.IsCompiledSuccessfully);
            Assert.IsNotNullOrEmpty(result.OutputFile);
            Assert.IsTrue(result.OutputFile.EndsWith(".exe"));
        }

        // TODO: This method is copied from ExecutionStrategy.cs. Extract it into separate class to reuse it. E.g. extension method to the Path class.
        protected string SaveStringToTempFile(string stringToWrite)
        {
            var tempFilePath = Path.GetTempFileName();
            File.WriteAllText(tempFilePath, stringToWrite);
            return tempFilePath;
        }
    }
}
