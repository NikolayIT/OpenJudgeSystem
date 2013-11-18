namespace SandboxExecutorProofOfConcept
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;

    using OJS.Common.Extensions;
    using OJS.Workers.Executors;
    using OJS.Workers.Executors.Impersonation;
    using OJS.Workers.Executors.Process;

    internal class SandboxExecutorProofOfConceptProgram
    {
        private const string UserName = @"testcode";
        private const string Password = @"testcode1234";
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
            var restrictedProcessExecutor = new RestrictedProcessExecutor();
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
        
        private static void ExecuteProcessWithDifferentUser(string applicationPath, string textToWrite, int timeLimit, int memoryLimit)
        {
            try
            {
                var process = new DifferentUserProcessExecutor(applicationPath, Environment.UserDomainName, UserName, Password);
                process.Process.Exited += ProcessOnExited;
                process.SetTextToWrite(textToWrite);
                Console.WriteLine("Chars to write to the process: {0}", process.CharsToWrite.Length);
                Console.WriteLine("Starting sandbox target process...");
                var executionInfo = process.Start(timeLimit, memoryLimit);

                Console.WriteLine("================== Process output ==================");
                Console.WriteLine(executionInfo.StandardOutputContent);

                if (!string.IsNullOrEmpty(executionInfo.StandardErrorContent))
                {
                    Console.WriteLine("================== Process error ===================");
                    Console.WriteLine(executionInfo.StandardErrorContent);
                }

                // Process information
                Console.WriteLine("================== Process info ====================");
                Console.WriteLine(ProcessInfoFormat, "Id:", process.Process.Id);
                Console.WriteLine(ProcessInfoFormat, "HasExited:", process.Process.HasExited);
                Console.WriteLine(ProcessInfoFormat, "ExitCode:", process.Process.ExitCode);
                Console.WriteLine(ProcessInfoFormat, "Error code description:", new Win32Exception(process.Process.ExitCode).Message);
                Console.WriteLine(ProcessInfoFormat, "PriorityClass:", process.Process.PriorityClass);
                Console.WriteLine(ProcessInfoTimeFormat, "StartTime:", process.Process.StartTime);
                Console.WriteLine(ProcessInfoTimeFormat, "ExitTime:", process.Process.ExitTime);
                Console.WriteLine(ProcessInfoFormat, "PrivilegedProcessorTime:", process.Process.PrivilegedProcessorTime);
                Console.WriteLine(ProcessInfoFormat, "UserProcessorTime:", process.Process.UserProcessorTime);
                Console.WriteLine(ProcessInfoFormat, "TotalProcessorTime:", process.Process.TotalProcessorTime);
                Console.WriteLine(ProcessInfoFormat, "ExitTime - StartTime:", process.Process.ExitTime - process.Process.StartTime);
                //// When process is exited no access to: process.BasePriority, process.HandleCount, MaxWorkingSet, etc.
                Console.WriteLine("==================== More info =====================");
                Console.WriteLine(ProcessInfoFormat, "ProcessKilledBecauseOfTimeLimit:", executionInfo.ProcessKilledBecauseOfTimeLimit);
                Console.WriteLine("====================================================");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                //// throw;
            }
        }

        private static void ProcessOnExited(object sender, EventArgs eventArgs)
        {
            Console.WriteLine(ProcessInfoTimeFormat, "-- Process exited event:", DateTime.Now);
        }

        private static void ImpersonateCurrentProcess()
        {
            Console.WriteLine("WindowsIdentity.GetCurrent().Name: " + WindowsIdentity.GetCurrent().Name);

            Console.WriteLine();
            var context = new WrapperImpersonationContext("Nikolay-PC", UserName, Password);
            context.Enter();
            Console.WriteLine("... context.Enter(); ...");

            // Execute code under other uses context
            Console.WriteLine("WindowsIdentity.GetCurrent().Name: " + WindowsIdentity.GetCurrent().Name);
            Console.WriteLine("Environment.UserName: " + Environment.UserName);
            //// Process.Start(SandboxTargetExecutablePath);
            //// File.ReadAllText("SandboxExecutorProofOfConcept.exe.config");

            Console.WriteLine();
            context.Leave();
            Console.WriteLine("... context.Leave(); ...");
            Console.WriteLine("WindowsIdentity.GetCurrent().Name: " + WindowsIdentity.GetCurrent().Name);
        }
    }
}
