namespace OJS.Workers.Executors.Tests
{
    using System;
    using System.CodeDom.Compiler;
    using System.IO;

    using Microsoft.CSharp;
    using NUnit.Framework;

    using OJS.Workers.Common;
    using OJS.Workers.Executors;
    using OJS.Workers.Executors.Implementations;

    public abstract class BaseExecutorsTestClass
    {
        protected const int DefaultMemoryLimit = 32 * 1024 * 1024;
        protected const int DefaultTimeLimit = 100;
        private readonly string exeDirectory = $@"{Environment.CurrentDirectory}\Exe\";

        protected IExecutor RestrictedProcess =>
            this.CreateExecutor(ProcessExecutorType.Restricted);

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
            var results = codeProvider.CompileAssemblyFromSource(parameters, sourceString);
            foreach (var error in results.Errors)
            {
                Console.WriteLine(error.ToString());
            }

            Assert.IsFalse(results.Errors.HasErrors, "Code compilation contains errors!");
            return outputExePath;
        }

        protected IExecutor CreateExecutor(ProcessExecutorType type) =>
            new ProcessExecutorFactory(new TasksService())
                .CreateProcessExecutor(0, 0, type);
    }
}
