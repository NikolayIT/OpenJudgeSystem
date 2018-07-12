namespace OJS.Workers.ExecutionStrategies.BlockchainStrategies
{
    using System;

    using OJS.Common.Models;
    using OJS.Workers.Common;
    using OJS.Workers.Executors;

    public class SolidityCompileExecuteAndRunUnitTestsExecutionStrategy : ExecutionStrategy
    {
        public SolidityCompileExecuteAndRunUnitTestsExecutionStrategy(
            Func<CompilerType, string> getCompilerPathFunc,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(baseTimeUsed, baseMemoryUsed) =>
            this.GetCompilerPathFunc = getCompilerPathFunc;

        protected Func<CompilerType, string> GetCompilerPathFunc { get; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            IExecutor executor = new RestrictedProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);
            throw new NotImplementedException();
        }
    }
}