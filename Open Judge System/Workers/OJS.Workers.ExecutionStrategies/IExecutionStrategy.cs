namespace OJS.Workers.ExecutionStrategies
{
    public interface IExecutionStrategy
    {
        SubmissionsExecutorResult Execute(SubmissionsExecutorContext submission);
    }
}
