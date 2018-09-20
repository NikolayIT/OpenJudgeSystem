namespace OJS.Workers.ExecutionStrategies.BlockchainStrategies
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using OJS.Workers.Common;

    internal class GanacheCli : IDisposable
    {
        private const int MaxOutputLinesToReadOnStartUp = 1000;
        private const string GanacheCliListeningPattern = @"^Listening\s+on[^\r?\n]*\s+127\.0\.0\.1:";
        private const string ErrorRegexPattern = @"^Error:[.|\r|\n|\r\n|\w|\W]*";
        private const string PortInUseErrorKey = "EADDRINUSE";
        private readonly string nodeJsExecutablePath;
        private readonly string ganacheNodeCliPath;
        private int processId;
        private string errorMessage;

        public GanacheCli(string nodeJsExecutablePath, string ganacheNodeCliPath)
        {
            this.nodeJsExecutablePath = nodeJsExecutablePath;
            this.ganacheNodeCliPath = ganacheNodeCliPath;
        }

        public void Dispose()
        {
            var process = Process
                .GetProcesses(Environment.MachineName)
                .FirstOrDefault(p => p.Id == this.processId);

            if (process != null && !process.HasExited)
            {
                process.Kill();
            }
        }

        public void Listen(int portNumber)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(this.nodeJsExecutablePath)
                {
                    Arguments = $"{this.ganacheNodeCliPath} -p {portNumber}",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var started = process.Start();

                if (!started)
                {
                    throw new Exception("ganache-cli cannot be started.");
                }

                this.processId = process.Id;

                if (this.VerifyGanacheCliIsListening(process, portNumber))
                {
                    return;
                }

                if (!process.HasExited)
                {
                    process.Kill();
                }

                throw new Exception($"{this.errorMessage}. Please contact an administrator.");
            }
        }

        private bool VerifyGanacheCliIsListening(Process process, int portNumber)
        {
            var isListening = false;

            try
            {
                var successRegex = new Regex(GanacheCliListeningPattern + portNumber, RegexOptions.Multiline);
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

                    var canReadLine = task.Wait(Constants.DefaultProcessExitTimeOutMilliseconds);
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

                errorOutputTask.Wait(100);

                if (!string.IsNullOrEmpty(errorOutput))
                {
                    this.errorMessage = errorRegex.Match(errorOutput).Value;
                    return false;
                }
            }
            catch (AggregateException ae)
            {
                this.errorMessage = ae.Message;
            }
            catch (Exception ex)
            {
                this.errorMessage = ex.Message;
                return false;
            }

            return isListening;
        }
    }
}