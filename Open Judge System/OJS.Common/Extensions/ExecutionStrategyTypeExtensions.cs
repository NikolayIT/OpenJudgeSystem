namespace OJS.Common.Extensions
{
    using OJS.Workers.Common.Models;

    public static class ExecutionStrategyTypeExtensions
    {
        public static string GetFileExtension(this ExecutionStrategyType executionStrategyType)
        {
            switch (executionStrategyType)
            {
                case ExecutionStrategyType.CompileExecuteAndCheck:
                    return null; // The file extension depends on the compiler.
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndCheck:
                    return "js";
                case ExecutionStrategyType.JavaPreprocessCompileExecuteAndCheck:
                    return "java";
                case ExecutionStrategyType.PythonExecuteAndCheck:
                    return "py";
                default:
                    return null;
            }
        }
    }
}
