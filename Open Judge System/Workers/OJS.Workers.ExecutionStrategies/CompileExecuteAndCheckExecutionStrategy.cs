namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using OJS.Common.Models;
    using OJS.Workers.Checkers;
    using OJS.Workers.Common;
    using OJS.Workers.Compilers;
    using OJS.Workers.Executors;

    public class CompileExecuteAndCheckExecutionStrategy : ExecutionStrategyBase
    {
        private readonly Func<CompilerType, string> getCompilerPathFunc;

        public CompileExecuteAndCheckExecutionStrategy(Func<CompilerType, string> getCompilerPathFunc)
        {
            this.getCompilerPathFunc = getCompilerPathFunc;
        }

        public override SubmissionsExecutorResult Execute(SubmissionsExecutorContext submission)
        {
            var result = new SubmissionsExecutorResult();

            // 1. Save source to a file
            var sourceCodeFilePath = this.SaveStringToTempFile(submission.Code);

            // 2. Compile the file
            ICompiler compiler = Compiler.CreateCompiler(submission.CompilerType);
            var compilerPath = this.getCompilerPathFunc(submission.CompilerType);
            var compilerResult = compiler.Compile(compilerPath, sourceCodeFilePath, submission.AdditionalCompilerArguments);
            File.Delete(sourceCodeFilePath);
            if (!compilerResult.IsCompiledSuccessfully)
            {
                result.IsCompiledSuccessfully = false;
                result.CompilerComment = compilerResult.CompilerComment;

                return result;
            }

            var outputFile = compilerResult.OutputFile;

            // 3. Execute and check each test
            IExecutor executor = new RestrictedProcessExecutor();
            IChecker checker = Checker.CreateChecker(submission.CheckerAssemblyName, submission.CheckerTypeName, submission.CheckerParameter);
            result.TestResults = new List<TestResult>();
            foreach (var test in submission.Tests)
            {
                var processExecutionResult = executor.Execute(outputFile, test.Input, submission.TimeLimit, submission.MemoryLimit);
                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, processExecutionResult.ReceivedOutput);
                result.TestResults.Add(testResult);
            }

            // 4. Clean our mess
            File.Delete(outputFile);

            return result;
        }
        
        //private string GetCompilerPath(CompilerType type)
        //{
        //    throw new NotImplementedException();
        //    //switch (type)
        //    //{
        //    //    case CompilerType.None:
        //    //        return null;
        //    //    case CompilerType.CSharp:
        //    //        return Settings.CSharpCompilerPath;
        //    //    case CompilerType.MsBuild:
        //    //        throw new NotImplementedException("Compiler not supported.");
        //    //    case CompilerType.CPlusPlus:
        //    //        return Settings.CPlusPlusGccCompilerPath;
        //    //    case CompilerType.JavaScript:
        //    //        return null;
        //    //    case CompilerType.Java:
        //    //        throw new NotImplementedException("Compiler not supported.");
        //    //    default:
        //    //        throw new ArgumentOutOfRangeException("type");
        //    //}
        //}
    }
}
