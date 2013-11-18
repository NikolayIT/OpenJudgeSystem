namespace OJS.Workers.Executors
{
    using System;
    using System.Collections.Generic;

    using OJS.Workers.Common;

    public class NodeJsExecutor : IExecutor
    {
        public ProcessExecutionResult Execute(string fileName, string inputData, int timeLimit, int memoryLimit, IEnumerable<string> executionArguments)
        {
            throw new NotImplementedException();
        }
    }
}
