namespace OJS.Workers.ExecutionStrategies.BlockchainStrategies
{
    using System;

    public class SolidityCompileExecuteAndRunUnitTestsExecutionStrategy : ExecutionStrategy
    {
        public SolidityCompileExecuteAndRunUnitTestsExecutionStrategy(
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(baseTimeUsed, baseMemoryUsed)
        {
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            throw new NotImplementedException();
        }
    }
}