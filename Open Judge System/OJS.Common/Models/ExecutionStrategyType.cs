namespace OJS.Common.Models
{
    public enum ExecutionStrategyType
    {
        DoNothing = 0,
        CompileExecuteAndCheck = 1,
        NodeJsPreprocessExecuteAndCheck = 2,
        RemoteExecution = 3,
        JavaPreprocessCompileArchiveExecuteAndCheck = 4,
        PhpCgiExecuteAndCheck = 5
    }
}
