namespace SandboxExecutorProofOfConcept
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    using OJS.Common.Extensions;
    using OJS.Workers.Executors;
    using OJS.Workers.Executors.Implementations;
    using OJS.Workers.Executors.Process;

    internal class SandboxExecutorProofOfConceptProgram
    {
        private const string ProcessInfoFormat = "{0,-25} {1}";
        private const string ProcessInfoTimeFormat = "{0,-25} {1:O}";
        private static readonly string SandboxTargetExecutablePath = Environment.CurrentDirectory + @"\SandboxTarget.exe";

        private static void Main()
        {
            // TODO: Agents should be run with ProcessPriorityClass.RealTime
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            Console.WriteLine(Process.GetCurrentProcess().PriorityClass);

            // RunNodeJs();
            // ThreadWork();
            for (int i = 0; i < 1; i++)
            {
                var thread = new Thread(ThreadWork);
                thread.Start();
                Thread.Sleep(100);
            }

            //// ExecuteProcessWithDifferentUser(SandboxTargetExecutablePath, ("Ю".Repeat(1024) + "").Repeat(20 * 1024) + "\n", 2000, 256 * 1024 * 1024);

            Console.ReadLine();
        }

        private static void RunNodeJs()
        {
            const string NodeJsExe = @"C:\Program Files\nodejs\node.exe";
            const string JsFilePath = @"C:\Temp\code.js";
            const string JsFileWorkingDirectory = @"C:\Temp";
            File.WriteAllText(JsFilePath, GlobalConstants.SampleJavaScriptCode);

            var process = new RestrictedProcess(NodeJsExe, JsFileWorkingDirectory, new List<string>() { JsFilePath });
            process.StandardInput.WriteLine("Vasko" + Environment.NewLine + "Niki2" + Environment.NewLine + "Niki3");
            process.Start(1000, 100 * 1024 * 1024);
            process.StandardInput.Close();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            Console.WriteLine(output);
            Console.WriteLine(error);
            Console.WriteLine(process.ExitCode);
        }

        private static void ThreadWork()
        {
            StartRestrictedProcess(SandboxTargetExecutablePath, "Ю".Repeat(1024).Repeat(25 * 1024) + "\n", 5000, 256 * 1024 * 1024);
        }

        private static void StartRestrictedProcess(string applicationPath, string textToWrite, int timeLimit, int memoryLimit)
        {
            var restrictedProcessExecutor = CreateRestrictedProcessExecutor();
            Console.WriteLine("-------------------- Starting RestrictedProcess... --------------------");
            var result = restrictedProcessExecutor.Execute(applicationPath, textToWrite, timeLimit, memoryLimit);
            Console.WriteLine("------------ Process output: {0} symbols. First 2048 symbols: ------------ ", result.ReceivedOutput.Length);
            Console.WriteLine(result.ReceivedOutput.Substring(0, Math.Min(2048, result.ReceivedOutput.Length)));
            if (!string.IsNullOrWhiteSpace(result.ErrorOutput))
            {
                Console.WriteLine(
                    "------------ Process error: {0} symbols. First 2048 symbols: ------------ ",
                    result.ErrorOutput.Length);
                Console.WriteLine(result.ErrorOutput.Substring(0, Math.Min(2048, result.ErrorOutput.Length)));
            }

            Console.WriteLine("------------ Process info ------------ ");
            Console.WriteLine(ProcessInfoFormat, "Type:", result.Type);
            Console.WriteLine(ProcessInfoFormat, "ExitCode:", string.Format("{0} ({1})", result.ExitCode, new Win32Exception(result.ExitCode).Message));
            Console.WriteLine(ProcessInfoFormat, "Total time:", result.TimeWorked);
            Console.WriteLine(ProcessInfoFormat, "PrivilegedProcessorTime:", result.PrivilegedProcessorTime);
            Console.WriteLine(ProcessInfoFormat, "UserProcessorTime:", result.UserProcessorTime);
            Console.WriteLine(ProcessInfoFormat, "Memory:", string.Format("{0:0.00}MB", result.MemoryUsed / 1024.0 / 1024.0));
            Console.WriteLine(new string('-', 79));
        }

        private static OJS.Workers.Common.IExecutor CreateRestrictedProcessExecutor() =>
            new ProcessExecutorFactory(new TasksService())
                .CreateProcessExecutor(0, 0, ProcessExecutorType.Restricted);
    }
}
