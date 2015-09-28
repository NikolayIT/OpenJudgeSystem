namespace SandboxTarget
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Windows.Forms;

    internal class SandboxTargetProgram
    {
        [STAThread]
        private static void Main()
        {
            // JustDoSomeCpuWork();

            //// TODO: Console.OutputEncoding = Encoding.UTF8;

            WriteLine("Hi, I am an instance of SandboxTargetProgram!");
            WriteLine(string.Format("Environment.UserName: {0}", Environment.UserName));
            WriteLine(string.Format("Environment.CurrentDirectory: {0}", Environment.CurrentDirectory));
            WriteLine(string.Format("Environment.OSVersion: {0}", Environment.OSVersion));
            WriteLine(string.Format("Environment.Version: {0}", Environment.Version));
            WriteLine(string.Format("Process.GetCurrentProcess().Id: {0}", Process.GetCurrentProcess().Id));
            WriteLine(string.Format("Process.GetCurrentProcess().PriorityClass: {0}", Process.GetCurrentProcess().PriorityClass));

            //// ThreadStart();

            ReadWriteConsole();

            var actions = new[]
                              {
                                  new TryToExecuteParams(x => File.OpenWrite(x), "create file", "file.txt"),
                                  new TryToExecuteParams(x => File.OpenWrite(x), "create file", @"C:\file.txt"),
                                  new TryToExecuteParams(x => File.OpenWrite(x), "create file", @"C:\Windows\file.txt"),
                                  new TryToExecuteParams(x => File.OpenWrite(x), "create file", string.Format("C:\\Users\\{0}\\AppData\\LocalLow\\{1}", Environment.UserName, @"file.txt")),
                                  new TryToExecuteParams(x => File.OpenRead(x), "read file", @"C:\Windows\win.ini"),
                                  //// TODO: new TryToExecuteParams(x => Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"\Software\AppDataLow\").CreateSubKey("asd"), "create registry key", ""),
                                  new TryToExecuteParams(x => Process.Start(x), "start process", string.Format("{0}\\notepad.exe", Environment.SystemDirectory)), // Unit tested
                                  new TryToExecuteParams(x => Process.Start("shutdown", x), "run shutdown", "\\?"),
                                  new TryToExecuteParams(x => Console.Write(Process.GetProcesses().Count()), "count processes", "\\?"),
                                  new TryToExecuteParams(x => new TcpClient().Connect(x, 80), "open socket", "google.com"),
                                  new TryToExecuteParams(x => new WebClient().DownloadString(x), "access http resource", "http://google.com"),
                                  new TryToExecuteParams(Clipboard.SetText, "write to clipboard", "data"), // Unit tested
                                  new TryToExecuteParams(
                                      x =>
                                          {
                                              if (string.IsNullOrEmpty(Clipboard.GetText()))
                                              {
                                                  throw new Exception("Clipboard empty!");
                                              }
                                          },
                                          "read from clipboard",
                                          null), // Unit tested
                              };

            int tests = 0;
            int passed = 0;
            foreach (var action in actions)
            {
                tests++;
                var result = TryToExecute(action);
                if (!result)
                {
                    passed++;
                }
            }

            Console.WriteLine("*** {0} tests run. {1} tests passed. {2} tests failed!!!", tests, passed, tests - passed);

            // ReadWrite10000LinesFromConsole();
            // Write10000SymbolsOnSingleLine();

            // for (int i = 0; i < 70; i++)
            // {
            //    Sleep(1);
            // }

            // RecursivelyCheckFileSystemPermissions(@"C:\", 0);
            // CreateInfiniteAmountOfMemory();

            // ThrowException();
            // InfiniteLoop();
        }

        private static void JustDoSomeCpuWork()
        {
            var data = new long[512];

            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 0; j < data.Length; j++)
                {
                    for (int k = 0; k < data.Length; k++)
                    {
                        data[i] += j * k;
                    }
                }
            }

            Console.WriteLine(data[data.Length - 1]);

            Environment.Exit(0);
        }

        private static void ThreadStart()
        {
            var threads = new List<Thread>();
            for (int i = 1; i < 100000; i++)
            {
                var thread = new Thread(() => Thread.Sleep(100000));
                threads.Add(thread);
                thread.Start();
                Console.WriteLine("Thread {0} started", i);
            }
        }

        private static void RecursivelyCheckFileSystemPermissions(string currentDirectory, int level)
        {
            Console.Write("{0}\\{1}\\ ", new string(' ', level * 4), new DirectoryInfo(currentDirectory).Name);

            try
            {
                Directory.GetFiles(currentDirectory);
                Console.Write("++FILES");
            }
            catch
            {
                Console.Write("--FILES");
            }

            try
            {
                var directories = Directory.GetDirectories(currentDirectory);
                Console.WriteLine("++DIRS");
                foreach (var directory in directories)
                {
                    RecursivelyCheckFileSystemPermissions(directory, level + 1);
                }
            }
            catch
            {
                Console.WriteLine("--DIRS");
            }
        }

        private static void InfiniteLoop()
        {
            WriteLine("Going to infinite loop... bye...");
            while (true)
            {
            }
        }

        private static bool TryToExecute(TryToExecuteParams tryToExecuteParams)
        {
            Write(string.Format("* {0} ({1}): ", tryToExecuteParams.Name, tryToExecuteParams.Parameter));
            try
            {
                tryToExecuteParams.Action(tryToExecuteParams.Parameter);
                WriteLine(" !!!Success!!!", ConsoleColor.Yellow);
                return true;
            }
            catch (Exception ex)
            {
                WriteLine(string.Format(" Exception: {0}", ex.Message), ConsoleColor.Red);
                return false;
            }
        }

        private static void ReadWriteConsole()
        {
            var startTime = DateTime.Now;
            var line = Console.ReadLine();
            var endTime = DateTime.Now;
            Console.WriteLine("ReadWriteConsole time: {0}", endTime - startTime);
            Console.WriteLine("Memory consumption: {0:0.000} MB", GC.GetTotalMemory(false) / 1024.0 / 1024.0);
            WriteLine(string.Format("Input size: {0}. First 5 chars: {1}", line.Length, line.Substring(0, 5)), ConsoleColor.White);
        }

        private static void ReadWrite10000LinesFromConsole()
        {
            var startTime = DateTime.Now;
            for (int i = 0; i < 10000; i++)
            {
                var line = Console.ReadLine();
                Console.WriteLine(line);
            }

            var endTime = DateTime.Now;
            Console.WriteLine("Read10000LinesFromConsole time: {0}", endTime - startTime);
            Console.WriteLine("Memory consumption: {0:0.000} MB", GC.GetTotalMemory(false) / 1024.0 / 1024.0);
        }

        private static void Write10000SymbolsOnSingleLine()
        {
            Console.Write(new string('Я', 10 * 1024 * 1024));
        }

        private static void Sleep(double seconds)
        {
            Write(string.Format("Going to sleep for {0} seconds... ", seconds));
            Thread.Sleep((int)(1000.0 * seconds));
            WriteLine(string.Format("Slept {0} seconds!", seconds), ConsoleColor.Yellow);
        }

        private static void CreateInfiniteAmountOfMemory()
        {
            Console.WriteLine("Start to infinite memory allocation...");
            var references = new List<object>();
            while (true)
            {
                Console.WriteLine("Using: {0:0.000} MB.", GC.GetTotalMemory(false) / 1024M / 1024M);
                references.Add(new int[250000]);
            }
        }

        private static void ThrowException()
        {
            Write("Throwing exception...", ConsoleColor.DarkGray);
            throw new Win32Exception("This text should be hidden for the user.");
        }

        private static void Write(string text = "", ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.Write(text);
        }

        private static void WriteLine(string text = "", ConsoleColor foregroundColor = ConsoleColor.Gray, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.WriteLine(text);
        }
    }
}
