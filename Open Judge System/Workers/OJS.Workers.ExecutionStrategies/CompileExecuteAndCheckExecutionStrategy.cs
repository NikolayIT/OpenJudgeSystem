namespace OJS.Workers.ExecutionStrategies
{
    using System;

    using OJS.Common.Models;
    using OJS.Workers.Common;
    using OJS.Workers.Executors;

    public class CompileExecuteAndCheckExecutionStrategy : ExecutionStrategy
    {
        public CompileExecuteAndCheckExecutionStrategy(Func<CompilerType, string> getCompilerPathFunc)
        {
            this.GetCompilerPathFunc = getCompilerPathFunc;
        }

        protected Func<CompilerType, string> GetCompilerPathFunc { get; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            IExecutor executor = new RestrictedProcessExecutor();
            var result = this.CompileExecuteAndCheck(executionContext, this.GetCompilerPathFunc, executor);
            return result;
        }
    }
}
