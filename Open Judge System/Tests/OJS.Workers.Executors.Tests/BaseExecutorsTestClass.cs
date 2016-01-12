namespace OJS.Workers.Executors.Tests
{
    using System;
    using System.CodeDom.Compiler;
    using System.IO;

    using Microsoft.CSharp;
    using NUnit.Framework;

    public abstract class BaseExecutorsTestClass
    {
        private readonly string exeDirectory = string.Format(@"{0}\Exe\", Environment.CurrentDirectory);

        public string CreateExe(string exeName, string sourceString)
        {
            Directory.CreateDirectory(this.exeDirectory);
            var outputExePath = this.exeDirectory + exeName;
            if (File.Exists(outputExePath))
            {
                File.Delete(outputExePath);
            }

            var codeProvider = new CSharpCodeProvider();
            var parameters = new CompilerParameters
                                 {
                                     GenerateExecutable = true,
                                     OutputAssembly = outputExePath,
                                 };
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, sourceString);
            foreach (var error in results.Errors)
            {
                Console.WriteLine(error.ToString());
            }

            Assert.IsFalse(results.Errors.HasErrors, "Code compilation contains errors!");
            return outputExePath;
        }
    }
}
