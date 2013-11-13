namespace TestSubmissionResponder
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading;

    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Workers.ExecutionStrategies;

    using ExecutionContext = OJS.Workers.ExecutionStrategies.ExecutionContext;

    public class Program
    {
        public static void Main(string[] args)
        {
            var data = new OjsData();

            // Submission.Processed should be index for performance considerations!

            while (true)
            {
                var dbSubmission =
                    data.Submissions.All()
                        .Where(x => !x.Processed)
                        .OrderBy(x => x.Id)
                        .Include(x => x.Problem)
                        .Include(x => x.Problem.Checker)
                        .Include(x => x.SubmissionType)
                        .FirstOrDefault();

                if (dbSubmission == null)
                {
                    Thread.Sleep(500);
                    continue;
                }

                Console.Write("Working on submission №{0}... ", dbSubmission.Id);

                IExecutionStrategy executionStrategy = CreateExecutionStrategy(dbSubmission.SubmissionType.ExecutionStrategyType);
                var context = new ExecutionContext
                                  {
                                    AdditionalCompilerArguments  = dbSubmission.SubmissionType.AdditionalCompilerArguments,
                                    CheckerAssemblyName = dbSubmission.Problem.Checker.DllFile,
                                    CheckerParameter = dbSubmission.Problem.Checker.Parameter,
                                    CheckerTypeName =  dbSubmission.Problem.Checker.ClassName,
                                    Code = dbSubmission.ContentAsString,
                                    CompilerType = dbSubmission.SubmissionType.CompilerType,
                                    MemoryLimit = dbSubmission.Problem.MemoryLimit,
                                    TimeLimit = dbSubmission.Problem.TimeLimit,
                                  };

                context.Tests = dbSubmission.Problem.Tests.ToList().Select(x => new TestContext
                                                                                {
                                                                                    Id = x.Id,
                                                                                    Input = x.InputDataAsString,
                                                                                    Output = x.OutputDataAsString,
                                                                                });

                var result = executionStrategy.Execute(context);

                foreach (var testRun in dbSubmission.TestRuns.ToList())
                {
                    data.TestRuns.Delete(testRun);
                }

                data.SaveChanges();

                dbSubmission.Processed = true;

                dbSubmission.IsCompiledSuccessfully = result.IsCompiledSuccessfully;
                dbSubmission.CompilerComment = result.CompilerComment;
                foreach (var testResult in result.TestResults)
                {
                    var testRun = new TestRun
                                      {
                                          CheckerComment = testResult.CheckerComment,
                                          ExecutionComment = testResult.ExecutionComment,
                                          MemoryUsed = testResult.MemoryUsed,
                                          ResultType = testResult.ResultType,
                                          TestId = testResult.Id,
                                          TimeUsed = testResult.TimeUsed,
                                      };
                    dbSubmission.TestRuns.Add(testRun);
                }

                // TODO: dbSubmission.Points

                data.SaveChanges();
                Console.WriteLine("Done!");
                Thread.Sleep(500);
            }
        }

        public static IExecutionStrategy CreateExecutionStrategy(ExecutionStrategyType type)
        {
            IExecutionStrategy executionStrategy;
            switch (type)
            {
                case ExecutionStrategyType.CompileExecuteAndCheck:
                    executionStrategy = new CompileExecuteAndCheckExecutionStrategy(GetCompilerPath);
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndCheck:
                    executionStrategy = new NodeJsPreprocessExecuteAndCheckExecutionStrategy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return executionStrategy;
        }

       public static string GetCompilerPath(CompilerType type)
       {
           switch (type)
           {
               case CompilerType.None:
                   return null;
               case CompilerType.CSharp:
                   return Settings.CSharpCompilerPath;
               case CompilerType.MsBuild:
                   throw new NotImplementedException("Compiler not supported.");
               case CompilerType.CPlusPlusGcc:
                   return Settings.CPlusPlusGccCompilerPath;
               case CompilerType.Java:
                   throw new NotImplementedException("Compiler not supported.");
               default:
                   throw new ArgumentOutOfRangeException("type");
           }
       }
    }
}
