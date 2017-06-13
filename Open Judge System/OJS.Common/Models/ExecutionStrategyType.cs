namespace OJS.Common.Models
{
    public enum ExecutionStrategyType
    {
        DoNothing = 0,
        CompileExecuteAndCheck = 1,
        CSharpUnitTestsExecutionStrategy = 23,
        CSharpProjectTestsExecutionStrategy = 24,
        CSharpAspProjectTestsExecutionStrategy = 28,
		CSharpPerformanceProjectTestsExecutionStrategy = 31,
        CSharpTestRunner = 10,
        CPlusPlusZipFileExecutionStrategy = 26,
        CPlusPlusCompileExecuteAndCheckExecutionStrategy = 29,
        NodeJsPreprocessExecuteAndCheck = 2,
        NodeJsPreprocessExecuteAndRunUnitTestsWithMocha = 11,
        NodeJsPreprocessExecuteAndRunJsDomUnitTests = 12,
        NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy = 19,
        NodeJsZipPreprocessExecuteAndRunUnitTestsWithDomAndMocha = 20,
        NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy = 21,
        NodeJsZipExecuteHtmlAndCssStrategy = 22,
        RemoteExecution = 3,
        JavaPreprocessCompileExecuteAndCheck = 4,
        JavaZipFileCompileExecuteAndCheck = 8,
        JavaProjectTestsExecutionStrategy = 25,
        JavaUnitTestsExecutionStrategy = 27,
 		JavaSpringAndHibernateProjectExecutionStrategy = 30
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
 		RubyExecutionStrategy = 29    }
}
