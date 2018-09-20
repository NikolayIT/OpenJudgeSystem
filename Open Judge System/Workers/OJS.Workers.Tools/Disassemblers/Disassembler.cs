namespace OJS.Workers.Tools.Disassemblers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;

    using OJS.Workers.Common;
    using OJS.Workers.Tools.Disassemblers.Contracts;

    public abstract class Disassembler : IDisassembler
    {
        protected const int ProcessSuccessExitCode = 0;

        protected Disassembler(string disassemblerPath)
        {
            if (!File.Exists(disassemblerPath))
            {
                throw new ArgumentException(
                    $"Disassembler not found in: {disassemblerPath}",
                    nameof(disassemblerPath));
            }

            this.DisassemblerPath = disassemblerPath;
        }

        protected string DisassemblerPath { get; }

        public DisassembleResult Disassemble(string compiledFilePath, string additionalArguments = null)
        {
            if (!File.Exists(compiledFilePath))
            {
                throw new ArgumentException(
                    $"Compiled file not found in: {compiledFilePath}.",
                    nameof(compiledFilePath));
            }

            var workingDirectory = new FileInfo(this.DisassemblerPath).DirectoryName;

            var arguments = this.BuildDisassemblerArguments(compiledFilePath, additionalArguments);

            var disassemblerProcessStartInfo =
                new ProcessStartInfo(this.DisassemblerPath)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = workingDirectory,
                    Arguments = arguments
                };

            this.UpdateDisassemblerProcessStartInfo(disassemblerProcessStartInfo);

            string disassambledCode;
            var isDisassembledSuccessfully =
                this.ExecuteDisassemblerProcess(disassemblerProcessStartInfo, out disassambledCode);

            return new DisassembleResult(isDisassembledSuccessfully, disassambledCode);
        }

        protected abstract string BuildDisassemblerArguments(string inputFilePath, string additionalArguments);

        protected virtual void UpdateDisassemblerProcessStartInfo(ProcessStartInfo disassemblerProcessStartInfo)
        {
        }

        protected virtual bool ExecuteDisassemblerProcess(
            ProcessStartInfo disassemblerProcessStartInfo,
            out string disassembledCode)
        {
            disassembledCode = null;

            using (var outputWaitHandle = new AutoResetEvent(false))
            {
                using (var process = new Process())
                {
                    process.StartInfo = disassemblerProcessStartInfo;

                    var outputBuilder = new StringBuilder();
                    process.OutputDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                outputWaitHandle.Set();
                            }
                            else
                            {
                                outputBuilder.AppendLine(e.Data);
                            }
                        };

                    var started = process.Start();
                    if (started)
                    {
                        process.BeginOutputReadLine();

                        var exited = process.WaitForExit(Constants.DefaultProcessExitTimeOutMilliseconds);
                        if (exited)
                        {
                            outputWaitHandle.WaitOne(100);

                            if (process.ExitCode == ProcessSuccessExitCode)
                            {
                                disassembledCode = outputBuilder.ToString().Trim();
                                return true;
                            }
                        }
                        else
                        {
                            process.CancelOutputRead();

                            // Double check if the process has exited before killing it
                            if (!process.HasExited)
                            {
                                process.Kill();
                            }
                        }
                    }

                    return false;
                }
            }
        }
    }
}
