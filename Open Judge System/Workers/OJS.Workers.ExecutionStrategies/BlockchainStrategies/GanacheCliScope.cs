namespace OJS.Workers.ExecutionStrategies.BlockchainStrategies
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using OJS.Common;

    internal class GanacheCliScope : IDisposable
    {
        private const int MaxOutputLinesToReadOnStartUp = 1000;
        private const string ErrorRegexPattern = @"^Error:[.|\r|\n|\r\n|\w|\W]*";
        private const string PortInUseErrorKey = "EADDRINUSE";
        private readonly string nodeJsExecutablePath;
        private readonly string ganacheNodeCliPath;
        private readonly int portNumber;
        private readonly int accountsCount;
        private readonly int processId;
        private string errorMessage;

        public GanacheCliScope(
            string nodeJsExecutablePath,
            string ganacheNodeCliPath,
            int portNumber,
            int accountsCount = 10)
        {
            this.nodeJsExecutablePath = nodeJsExecutablePath;
            this.ganacheNodeCliPath = ganacheNodeCliPath;
            this.portNumber = portNumber;
            this.accountsCount = accountsCount;

            this.processId = this.RunGanacheCli();
        }

        private string GanacheCliListeningPattern => $@"^Listening\s+on[^\r?\n]*\s+127\.0\.0\.1:{this.portNumber}";

        public void Dispose()
        {
            var process = Process.GetProcesses(Environment.MachineName).FirstOrDefault(p => p.Id == this.processId);

            if (process != null && !process.HasExited)
            {
                process.Kill();
            }
        }

        private int RunGanacheCli()
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(this.nodeJsExecutablePath)
                {
                    Arguments = $"{this.ganacheNodeCliPath} -p {this.portNumber} -a {this.accountsCount}",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var started = process.Start();

                if (!started)
                {
                    this.errorMessage = "ganache-cli cannot be started";
                }
                else if (this.VerifyGanacheCliIsListening(process))
                {
                    return process.Id;
                }
                else if (!process.HasExited)
                {
                    process.Kill();
                }

                throw new Exception($"{this.errorMessage}. Please contact an administrator.");
            }
        }

        private bool VerifyGanacheCliIsListening(Process process)
        {
            var isListening = false;

            try
            {
                var successRegex = new Regex(this.GanacheCliListeningPattern, RegexOptions.Multiline);
                var errorRegex = new Regex(ErrorRegexPattern, RegexOptions.Multiline);
                var errorOutput = string.Empty;

                var errorOutputTask = process.StandardError.ReadToEndAsync().ContinueWith(
                    x =>
                    {
                        errorOutput = x.Result;
                    });

                var linesCount = 0;
                while (true)
                {
                    // ReSharper disable once AccessToDisposedClosure
                    var task = Task.Factory.StartNew(() => process.StandardOutput.ReadLine());

                    var canReadLine = task.Wait(GlobalConstants.DefaultProcessExitTimeOutMilliseconds);
                    if (!canReadLine)
                    {
                        this.errorMessage = "ganache-cli is unresponsive";
                        break;
                    }

                    var outputLine = task.Result;

                    if (outputLine == null)
                    {
                        // Standard error output task will pick up the error message
                        break;
                    }

                    if (errorRegex.IsMatch(outputLine))
                    {
                        this.errorMessage = errorRegex.Match(outputLine).Value;
                        if (this.errorMessage.Contains(PortInUseErrorKey))
                        {
                            this.errorMessage = "Requested port is in use";
                        }

                        break;
                    }

                    if (successRegex.IsMatch(outputLine))
                    {
                        isListening = true;
                        break;
                    }

                    if (++linesCount <= MaxOutputLinesToReadOnStartUp)
                    {
                        continue;
                    }

                    this.errorMessage = "ganache-cli exceeded the limit of startup output lines";
                    break;
                }

                try
                {
                    errorOutputTask.Wait(100);

                    if (!string.IsNullOrEmpty(errorOutput))
                    {
                        this.errorMessage = errorRegex.Match(errorOutput).Value;
                    }
                }
                catch (AggregateException ex)
                {
                    this.errorMessage = ex.Message;
                }
            }
            catch (Exception ex)
            {
                this.errorMessage = ex.Message;
            }

            return isListening;
        }
    }
}