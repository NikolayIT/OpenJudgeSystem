namespace OJS.Workers.ExecutionStrategies
{
    public class NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy : NodeJsPreprocessExecuteAndCheckExecutionStrategy
    {
        public NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy(string nodeJsExecutablePath)
            : base(nodeJsExecutablePath)
        {
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            throw new System.NotImplementedException();
        }
    }
}
