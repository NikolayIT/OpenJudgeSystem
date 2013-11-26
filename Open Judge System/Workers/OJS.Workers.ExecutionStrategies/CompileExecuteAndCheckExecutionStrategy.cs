namespace OJS.Workers.ExecutionStrategies
{
    using System;

    using OJS.Common.Models;
    using OJS.Workers.Common;
    using OJS.Workers.Executors;

    public class CompileExecuteAndCheckExecutionStrategy : ExecutionStrategy
    {
        private readonly Func<CompilerType, string> getCompilerPathFunc;

        public CompileExecuteAndCheckExecutionStrategy(Func<CompilerType, string> getCompilerPathFunc)
        {
            this.getCompilerPathFunc = getCompilerPathFunc;
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            IExecutor executor = new RestrictedProcessExecutor();
            var result = this.CompileExecuteAndCheck(executionContext, this.getCompilerPathFunc, executor);
            return result;
        }
    }
}
