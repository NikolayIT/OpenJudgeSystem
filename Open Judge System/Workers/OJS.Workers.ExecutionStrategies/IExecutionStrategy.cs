namespace OJS.Workers.ExecutionStrategies
{
    public interface IExecutionStrategy
    {
        ExecutionResult SafeExecute(ExecutionContext executionContext);

        ExecutionResult Execute(ExecutionContext executionContext);
    }
}
