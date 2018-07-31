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
        private readonly string nodeJsExecutablePath;
        private readonly string ganacheNodeCliPath;
        private readonly int portNumber;
        private readonly int accountsCount;
        private readonly int processId;

        public GanacheCliScope(
            string nodeJsExecutablePath,
            string ganacheNodeCliPath,
            int portNumber,
            int accountsCount = 1)
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
            var successRegex = new Regex(this.GanacheCliListeningPattern, RegexOptions.Multiline);
            var errorRegex = new Regex(ErrorRegexPattern, RegexOptions.Multiline);
            var errorMessage = string.Empty;
            var errorOutput = string.Empty;
            var isListening = false;

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
                    throw new Exception("ganache-cli cannot be started");
                }

                var errorOutputTask = process.StandardError.ReadToEndAsync().ContinueWith(
                    x =>
                    {
                        errorOutput = x.Result;
                    });

                // Verify that ganache-cli is running and listening on the port
                var linesCount = 0;
                while (true)
                {
                    // ReSharper disable once AccessToDisposedClosure
                    var task = Task.Factory.StartNew(() => process.StandardOutput.ReadLine());

                    var canReadLine = task.Wait(GlobalConstants.DefaultProcessExitTimeOutMilliseconds);
                    if (!canReadLine)
                    {
                        errorMessage = "ganache-cli is unresponsive";
                    }

                    var outputLine = task.Result;

                    if (outputLine == null)
                    {
                        // Standard error output task will pick the error message
                        break;
                    }

                    if (errorRegex.IsMatch(outputLine))
                    {
                        errorMessage = errorRegex.Match(outputLine).Value;
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

                    errorMessage = "ganache-cli exceeded the limit of startup output lines";
                    break;
                }

                try
                {
                    errorOutputTask.Wait(100);

                    if (!string.IsNullOrEmpty(errorOutput))
                    {
                        errorMessage = errorRegex.Match(errorOutput)?.Value;
                    }
                }
                catch (AggregateException ex)
                {
                    errorMessage = ex.Message;
                }

                if (isListening)
                {
                    return process.Id;
                }

                process.WaitForExit(GlobalConstants.DefaultProcessExitTimeOutMilliseconds);

                if (!process.HasExited)
                {
                    process.Kill();
                }

                throw new Exception(errorMessage);
            }
        }
    }
}