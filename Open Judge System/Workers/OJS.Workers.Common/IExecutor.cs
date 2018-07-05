namespace OJS.Workers.Common
{
    using System.Collections.Generic;

    public interface IExecutor
    {
        /// <summary>
        /// Runs a specified executable in a restricted process with time and memory limitations, memory usage sampling and
        /// redirected input and output streams in order to pass and read information from it.
        /// </summary>
        /// <param name="fileName">The path to the executable</param>
        /// <param name="inputData">Information that should be written on the Standard Input of the process.</param>
        /// <param name="timeLimit">Time limit of the process in miliseconds.</param>
        /// <param name="memoryLimit">Memory limit of the process in bytes.</param>
        /// <param name="executionArguments">Additional command line arguments that should be passed to the executable.</param>
        /// <param name="workingDirectory">The working directory of the process.</param>
        /// <param name="useProcessTime">A boolean value indicating whether the Process's time or the Total Processor time
        /// should be used when calculating the total time used by the Process.</param>
        /// <param name="useSystemEncoding">A boolean value indicating whether the redirected Input and Output streams
        /// should use the default System Encoding or use UTF-8</param>
        /// <param name="dependOnExitCodeForRunTimeError">A boolean value indicating whether the executor should consider an exit code
        /// lower that -1 as a RunTime error if the ErrorOutput is empty</param>
        /// <param name="timeoutMultiplier">A multiplier for the timeLimit, the total execution limit for the process is equal
        /// to the <param name="timeLimit">timeLimit</param> * <param name="timeoutMultiplier">timeoutMultiplier</param></param>
        /// <returns></returns>
        ProcessExecutionResult Execute(
            string fileName,
            string inputData,
            int timeLimit,
            int memoryLimit,
            IEnumerable<string> executionArguments = null,
            string workingDirectory = null,
            bool useProcessTime = false,
            bool useSystemEncoding = false,
            bool dependOnExitCodeForRunTimeError = false,
            double timeoutMultiplier = 1.5);
    }
}