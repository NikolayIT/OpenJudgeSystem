namespace OJS.Workers.ExecutionStrategies
{
    using System;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Models;
    using OJS.Workers.Executors;

    public class CPlusPlusCompileExecuteAndCheckExecutionStrategy : CompileExecuteAndCheckExecutionStrategy
    {
        public CPlusPlusCompileExecuteAndCheckExecutionStrategy(
            Func<CompilerType, string> getCompilerPathFunc,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(getCompilerPathFunc, baseTimeUsed, baseMemoryUsed)
        {
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            IExecutor executor = new RestrictedProcessExecutor(this.BaseTimeUsed, this.BaseMemoryUsed);

            var result = this.CompileExecuteAndCheck(
                executionContext,
                this.GetCompilerPathFunc,
                executor,
                useSystemEncoding: false,
                dependOnExitCodeForRunTimeError: true);

            return result;
        }
    }
}
