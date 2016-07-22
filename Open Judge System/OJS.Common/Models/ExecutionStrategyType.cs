namespace OJS.Common.Models
{
    public enum ExecutionStrategyType
    {
        DoNothing = 0,
        CompileExecuteAndCheck = 1,
        CSharpTestRunner = 10,
        NodeJsPreprocessExecuteAndCheck = 2,
        NodeJsPreprocessExecuteAndRunUnitTestsWithMocha = 11,
        IoJsPreprocessExecuteAndRunJsDomUnitTests = 12,
        RemoteExecution = 3,
        JavaPreprocessCompileExecuteAndCheck = 4,
        JavaZipFileCompileExecuteAndCheck = 8,
        PythonExecuteAndCheck = 9,
        PhpCgiExecuteAndCheck = 5,
        PhpCliExecuteAndCheck = 6,
        CheckOnly = 7,
        SqlServerLocalDbPrepareDatabaseAndRunQueries = 13,
        SqlServerLocalDbRunQueriesAndCheckDatabase = 14,
        SqlServerLocalDbRunSkeletonRunQueriesAndCheckDatabase = 15,
        MySqlPrepareDatabaseAndRunQueries = 16,
        MySqlRunQueriesAndCheckDatabase = 17,
        MySqlRunSkeletonRunQueriesAndCheckDatabase = 18,
    }
}
