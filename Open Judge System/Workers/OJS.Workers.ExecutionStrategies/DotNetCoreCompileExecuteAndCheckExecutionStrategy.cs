namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using OJS.Common.Models;

    public class DotNetCoreCompileExecuteAndCheckExecutionStrategy : CompileExecuteAndCheckExecutionStrategy
    {
        public DotNetCoreCompileExecuteAndCheckExecutionStrategy(
            Func<CompilerType, string> getCompilerPathFunc,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(getCompilerPathFunc, baseTimeUsed, baseMemoryUsed)
        {
        }
    }
}