namespace OJS.Workers.ExecutionStrategies
{
    public interface IExecutionStrategy
    {
        SubmissionsExecutorResult Execute(ExecutionContext executionContext);
    }
}
