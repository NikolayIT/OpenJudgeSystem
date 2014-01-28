namespace OJS.Workers.Executors.Tests
{
    using System;
    using System.CodeDom.Compiler;
    using System.IO;

    using Microsoft.CSharp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public abstract class BaseExecutorsTestClass
    {
        protected const string TimeLimitSourceCode = @"using System;
using System.Threading;
class Program
{
    public static void Main()
    {
        Thread.Sleep(150);
    }
}";

        protected const string ReadInputAndThenOutputSourceCode = @"using System;
class Program
{
    public static void Main()
    {
        var line = Console.ReadLine();
        Console.WriteLine(line);
    }
}";

        protected const string ReadInputAndThenOutputTheLengthSourceCode = @"using System;
class Program
{
    public static void Main()
    {
        var line = Console.ReadLine();
        Console.WriteLine(line.Length);
    }
}";

        protected const string ReadInputAndThenCheckTheTextToContainCyrillicLettersSourceCode = @"using System;
class Program
{
    public static void Main()
    {
        var line = Console.ReadLine();
        Console.WriteLine((line.Contains(""а"") || line.Contains(""е"")));
    }
}";

        protected const string ReadClipboardSourceCode = @"using System;
using System.Windows.Forms;
class Program
{
    public static void Main()
    {
        if (string.IsNullOrEmpty(Clipboard.GetText()))
        {
            throw new Exception(""Clipboard empty!"");
        }
    }
}";

        protected const string WriteToClipboardSourceCode = @"using System;
using System.Windows.Forms;
class Program
{
    public static void Main()
    {
        Clipboard.SetText(""i did it"");
    }
}";

        protected const string StartNotepadProcessSourceCode = @"using System;
using System.Diagnostics;
class Program
{
    public static void Main()
    {
        Process.Start(string.Format(""{0}\\notepad.exe"", Environment.SystemDirectory));
    }
}";

        protected const string EndlessLoopSourceCode = @"using System;
class Program
{
    public static void Main()
    {
        while(true) { }
    }
}";

        protected const string ThrowExceptionSourceCode = @"using System;
using System.Windows.Forms;
class Program
{
    public static void Main()
    {
        throw new Exception(""Exception message!"");
    }
}";

        protected const string Consuming50MbOfMemorySourceCode = @"using System;
using System.Windows.Forms;
class Program
{
    public static void Main()
    {
        var array = new int[50 * 1024 * 1024 / 4];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = i;
        }
        Console.WriteLine(array[12345]);
    }
}";

        protected readonly string ExeDirectory = string.Format(@"{0}\Exe\", Environment.CurrentDirectory);

        public string CreateExe(string exeName, string sourceString)
        {
            Directory.CreateDirectory(this.ExeDirectory);
            var outputExePath = this.ExeDirectory + exeName;
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
