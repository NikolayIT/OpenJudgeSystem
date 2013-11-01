namespace OJS.Workers.Common
{
    public enum ProcessExecutionResultType
    {
        Success = 0,
        TimeLimit = 1,
        MemoryLimit = 2,
        RunTimeError = 4,
    }
}
