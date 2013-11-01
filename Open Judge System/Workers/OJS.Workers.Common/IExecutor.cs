namespace OJS.Workers.Common
{
    public interface IExecutor
    {
        ProcessExecutionResult Execute(string fileName, string inputData, int timeLimit, int memoryLimit);
    }
}
