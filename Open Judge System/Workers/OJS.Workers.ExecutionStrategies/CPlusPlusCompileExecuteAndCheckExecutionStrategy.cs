namespace OJS.Workers.ExecutionStrategies
{
    using System;

    using OJS.Common.Models;
    using OJS.Workers.Common;
    using OJS.Workers.Executors;

    public class CPlusPlusCompileExecuteAndCheckExecutionStrategy : CompileExecuteAndCheckExecutionStrategy
    {
        public CPlusPlusCompileExecuteAndCheckExecutionStrategy(Func<CompilerType, string> getCompilerPathFunc)
            : base(getCompilerPathFunc)
        {
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            IExecutor executor = new RestrictedProcessExecutor();
            var result = this.CompileExecuteAndCheck(executionContext, this.GetCompilerPathFunc, executor, false);
            return result;
        }
    }
}
