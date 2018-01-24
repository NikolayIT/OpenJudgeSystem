namespace OJS.Workers.LocalWorker.Helpers
{
    using System;

    using OJS.Common.Models;
    using OJS.Workers.ExecutionStrategies;
    using OJS.Workers.ExecutionStrategies.SqlStrategies.MySql;
    using OJS.Workers.ExecutionStrategies.SqlStrategies.SqlServerLocalDb;

    public static class SubmissionJobHelper
    {
        public static IExecutionStrategy CreateExecutionStrategy(ExecutionStrategyType type)
        {
            IExecutionStrategy executionStrategy;
            switch (type)
            {
                case ExecutionStrategyType.CompileExecuteAndCheck:
                    executionStrategy = new CompileExecuteAndCheckExecutionStrategy(
                        GetCompilerPath,
                        Settings.MsBuildBaseTimeUsedInMilliseconds,
                        Settings.MsBuildBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.CPlusPlusCompileExecuteAndCheckExecutionStrategy:
                    executionStrategy = new CPlusPlusCompileExecuteAndCheckExecutionStrategy(
                        GetCompilerPath,
                        Settings.GccBaseTimeUsedInMilliseconds,
                        Settings.GccBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.CPlusPlusZipFileExecutionStrategy:
                    executionStrategy = new CPlusPlusZipFileExecutionStrategy(
                        GetCompilerPath,
                        Settings.GccBaseTimeUsedInMilliseconds,
                        Settings.GccBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.DotNetCoreTestRunner:
                    executionStrategy = new DotNetCoreTestRunnerExecutionStrategy(
                        GetCompilerPath,
                        Settings.DotNetCliBaseTimeUsedInMilliseconds,
                        Settings.DotNetCliBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.CSharpUnitTestsExecutionStrategy:
                    executionStrategy = new CSharpUnitTestsExecutionStrategy(
                        Settings.NUnitConsoleRunnerPath,
                        GetCompilerPath,
                        Settings.MsBuildBaseTimeUsedInMilliseconds,
                        Settings.MsBuildBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.CSharpProjectTestsExecutionStrategy:
                    executionStrategy = new CSharpProjectTestsExecutionStrategy(
                        Settings.NUnitConsoleRunnerPath,
                        GetCompilerPath,
                        Settings.MsBuildBaseTimeUsedInMilliseconds,
                        Settings.MsBuildBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.CSharpPerformanceProjectTestsExecutionStrategy:
                    executionStrategy = new CSharpPerformanceProjectTestsExecutionStrategy(
                        Settings.NUnitConsoleRunnerPath,
                        GetCompilerPath,
                        Settings.DotNetCliBaseTimeUsedInMilliseconds,
                        Settings.DotNetCliBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.CSharpAspProjectTestsExecutionStrategy:
                    executionStrategy = new CSharpAspProjectTestsExecutionStrategy(
                        Settings.NUnitConsoleRunnerPath,
                        GetCompilerPath,
                        Settings.MsBuildBaseTimeUsedInMilliseconds,
                        Settings.MsBuildBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.DotNetCoreProjectExecutionStrategy:
                    executionStrategy = new DotNetCoreProjectExecutionStrategy(
                        GetCompilerPath,
                        Settings.DotNetCliBaseTimeUsedInMilliseconds,
                        Settings.DotNetCliBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.DotNetCoreProjectTestsExecutionStrategy:
                    executionStrategy = new DotNetCoreProjectTestsExecutionStrategy(
                        GetCompilerPath,
                        Settings.DotNetCliBaseTimeUsedInMilliseconds,
                        Settings.DotNetCliBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.RubyExecutionStrategy:
                    executionStrategy = new RubyExecutionStrategy(
                        Settings.RubyPath,
                        Settings.RubyBaseTimeUsedInMilliseconds,
                        Settings.RubyBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.JavaPreprocessCompileExecuteAndCheck:
                    executionStrategy = new JavaPreprocessCompileExecuteAndCheckExecutionStrategy(
                        Settings.JavaExecutablePath,
                        GetCompilerPath,
                        Settings.JavaBaseTimeUsedInMilliseconds,
                        Settings.JavaBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.JavaZipFileCompileExecuteAndCheck:
                    executionStrategy = new JavaZipFileCompileExecuteAndCheckExecutionStrategy(
                        Settings.JavaExecutablePath,
                        GetCompilerPath,
                        Settings.JavaBaseTimeUsedInMilliseconds,
                        Settings.JavaBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.JavaProjectTestsExecutionStrategy:
                    executionStrategy = new JavaProjectTestsExecutionStrategy(
                        Settings.JavaExecutablePath,
                        GetCompilerPath,
                        Settings.JavaLibsPath,
                        Settings.JavaBaseTimeUsedInMilliseconds,
                        Settings.JavaBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.JavaUnitTestsExecutionStrategy:
                    executionStrategy = new JavaUnitTestsExecutionStrategy(
                        Settings.JavaExecutablePath,
                        GetCompilerPath,
                        Settings.JavaLibsPath,
                        Settings.JavaBaseTimeUsedInMilliseconds,
                        Settings.JavaBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.JavaSpringAndHibernateProjectExecutionStrategy:
                    executionStrategy = new JavaSpringAndHibernateProjectExecutionStrategy(
                        Settings.JavaExecutablePath,
                        GetCompilerPath,
                        Settings.JavaLibsPath,
                        Settings.MavenPath,
                        Settings.JavaBaseTimeUsedInMilliseconds,
                        Settings.JavaBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndCheck:
                    executionStrategy = new NodeJsPreprocessExecuteAndCheckExecutionStrategy(
                        Settings.NodeJsExecutablePath,
                        Settings.UnderscoreModulePath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds * 2,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndRunUnitTestsWithMocha:
                    executionStrategy = new NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy(
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsZipPreprocessExecuteAndRunUnitTestsWithDomAndMocha:
                    executionStrategy = new NodeJsZipPreprocessExecuteAndRunUnitTestsWithDomAndMochaExecutionStrategy(
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.JsDomModulePath,
                        Settings.JQueryModulePath,
                        Settings.HandlebarsModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.BrowserifyModulePath,
                        Settings.BabelifyModulePath,
                        Settings.Es2015ImportPluginPath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndRunJsDomUnitTests:
                    executionStrategy = new NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy(
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.JsDomModulePath,
                        Settings.JQueryModulePath,
                        Settings.HandlebarsModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy:
                    executionStrategy = new NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy(
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.JsDomModulePath,
                        Settings.JQueryModulePath,
                        Settings.HandlebarsModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy:
                    executionStrategy = new NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy(
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.JsDomModulePath,
                        Settings.JQueryModulePath,
                        Settings.HandlebarsModulePath,
                        Settings.SinonJsDomModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.BabelCoreModulePath,
                        Settings.ReactJsxPluginPath,
                        Settings.ReactModulePath,
                        Settings.ReactDomModulePath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.NodeJsZipExecuteHtmlAndCssStrategy:
                    executionStrategy = new NodeJsZipExecuteHtmlAndCssStrategy(
                        Settings.NodeJsExecutablePath,
                        Settings.MochaModulePath,
                        Settings.ChaiModulePath,
                        Settings.SinonModulePath,
                        Settings.SinonChaiModulePath,
                        Settings.JsDomModulePath,
                        Settings.JQueryModulePath,
                        Settings.UnderscoreModulePath,
                        Settings.BootstrapModulePath,
                        Settings.BootstrapCssPath,
                        Settings.NodeJsBaseTimeUsedInMilliseconds,
                        Settings.NodeJsBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.PythonExecuteAndCheck:
                    executionStrategy = new PythonExecuteAndCheckExecutionStrategy(
                        Settings.PythonExecutablePath,
                        Settings.PythonBaseTimeUsedInMilliseconds,
                        Settings.PythonBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.PhpProjectExecutionStrategy:
                    executionStrategy = new PhpProjectExecutionStrategy(
                        Settings.PhpCliExecutablePath,
                        Settings.PhpCliBaseTimeUsedInMilliseconds,
                        Settings.PhpCliBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.PhpProjectWithDbExecutionStrategy:
                    executionStrategy = new PhpProjectWithDbExecutionStrategy(
                        Settings.PhpCliExecutablePath,
                        Settings.MySqlSysDbConnectionString,
                        Settings.MySqlRestrictedUserId,
                        Settings.MySqlRestrictedUserPassword,
                        Settings.PhpCliBaseTimeUsedInMilliseconds,
                        Settings.PhpCliBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.PhpCgiExecuteAndCheck:
                    executionStrategy = new PhpCgiExecuteAndCheckExecutionStrategy(
                        Settings.PhpCgiExecutablePath,
                        Settings.PhpCgiBaseTimeUsedInMilliseconds,
                        Settings.PhpCgiBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.PhpCliExecuteAndCheck:
                    executionStrategy = new PhpCliExecuteAndCheckExecutionStrategy(
                        Settings.PhpCliExecutablePath,
                        Settings.PhpCliBaseTimeUsedInMilliseconds,
                        Settings.PhpCliBaseMemoryUsedInBytes);
                    break;
                case ExecutionStrategyType.SqlServerLocalDbPrepareDatabaseAndRunQueries:
                    executionStrategy = new SqlServerLocalDbPrepareDatabaseAndRunQueriesExecutionStrategy(
                        Settings.SqlServerLocalDbMasterDbConnectionString,
                        Settings.SqlServerLocalDbRestrictedUserId,
                        Settings.SqlServerLocalDbRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.SqlServerLocalDbRunQueriesAndCheckDatabase:
                    executionStrategy = new SqlServerLocalDbRunQueriesAndCheckDatabaseExecutionStrategy(
                        Settings.SqlServerLocalDbMasterDbConnectionString,
                        Settings.SqlServerLocalDbRestrictedUserId,
                        Settings.SqlServerLocalDbRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.SqlServerLocalDbRunSkeletonRunQueriesAndCheckDatabase:
                    executionStrategy = new SqlServerLocalDbRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy(
                        Settings.SqlServerLocalDbMasterDbConnectionString,
                        Settings.SqlServerLocalDbRestrictedUserId,
                        Settings.SqlServerLocalDbRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.MySqlPrepareDatabaseAndRunQueries:
                    executionStrategy = new MySqlPrepareDatabaseAndRunQueriesExecutionStrategy(
                        Settings.MySqlSysDbConnectionString,
                        Settings.MySqlRestrictedUserId,
                        Settings.MySqlRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.MySqlRunQueriesAndCheckDatabase:
                    executionStrategy = new MySqlRunQueriesAndCheckDatabaseExecutionStrategy(
                        Settings.MySqlSysDbConnectionString,
                        Settings.MySqlRestrictedUserId,
                        Settings.MySqlRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.MySqlRunSkeletonRunQueriesAndCheckDatabase:
                    executionStrategy = new MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy(
                        Settings.MySqlSysDbConnectionString,
                        Settings.MySqlRestrictedUserId,
                        Settings.MySqlRestrictedUserPassword);
                    break;
                case ExecutionStrategyType.DoNothing:
                    executionStrategy = new DoNothingExecutionStrategy();
                    break;
                case ExecutionStrategyType.RemoteExecution:
                    executionStrategy = new RemoteExecutionStrategy();
                    break;
                case ExecutionStrategyType.CheckOnly:
                    executionStrategy = new CheckOnlyExecutionStrategy(0, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return executionStrategy;
        }

        private static string GetCompilerPath(CompilerType type)
        {
            switch (type)
            {
                case CompilerType.None:
                    return null;
                case CompilerType.CSharp:
                    return Settings.CSharpCompilerPath;
                case CompilerType.MsBuild:
                case CompilerType.MsBuildLibrary:
                    return Settings.MsBuildExecutablePath;
                case CompilerType.CPlusPlusGcc:
                case CompilerType.CPlusPlusZip:
                    return Settings.CPlusPlusGccCompilerPath;
                case CompilerType.Java:
                case CompilerType.JavaZip:
                case CompilerType.JavaInPlaceCompiler:
                    return Settings.JavaCompilerPath;
                case CompilerType.DotNetCompiler:
                    return Settings.DotNetCompilerPath;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}