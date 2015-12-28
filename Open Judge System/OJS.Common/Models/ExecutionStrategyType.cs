namespace OJS.Common.Models
{
    public enum ExecutionStrategyType
    {
        DoNothing = 0,
        CompileExecuteAndCheck = 1,
        CSharpTestRunner = 9,
        NodeJsPreprocessExecuteAndCheck = 2,
        NodeJsPreprocessExecuteAndRunUnitTestsWithMocha = 7,
        IoJsPreprocessExecuteAndRunJsDomUnitTests = 8,
        RemoteExecution = 3,
        JavaPreprocessCompileExecuteAndCheck = 4,
        JavaZipFileCompileExecuteAndCheck = 10,
        PythonExecuteAndCheck = 11,
        PhpCgiExecuteAndCheck = 5,
        PhpCliExecuteAndCheck = 6
    }
}
