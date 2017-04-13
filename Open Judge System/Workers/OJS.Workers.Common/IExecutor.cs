namespace OJS.Workers.Common
{
    using System.Collections.Generic;

    public interface IExecutor
    {
        ProcessExecutionResult Execute(string fileName, string inputData, int timeLimit, int memoryLimit, IEnumerable<string> executionArguments = null, string workingDirectory = null);
    }
}
