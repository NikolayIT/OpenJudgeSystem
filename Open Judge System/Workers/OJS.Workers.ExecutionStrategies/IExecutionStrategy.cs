namespace OJS.Workers.ExecutionStrategies
{
    public interface IExecutionStrategy
    {
        ExecutionResult Execute(ExecutionContext executionContext);
    }
}
