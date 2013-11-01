namespace TestSubmissionResponder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using OJS.Data;
    using OJS.Data.Models;

    public class Program
    {
        public static string[] Comments = { "Ok", "Test comment", "Some other comment" };

        public static void Main(string[] args)
        {
            var data = new OjsData();
            var random = new Random();

            while (true)
            {
                var submissions = data.Submissions.AllWithDeleted().Where(x => x.TestRuns.Count == 0);

                if (submissions.Count() != 0)
                {
                    Console.WriteLine("Found submissions.");
                }

                foreach (var submission in submissions)
                {
                    var problem = submission.Problem;
                    if (problem.Tests.Count == 0)
                    {
                        Console.WriteLine("Generating tests for problem.");
                        for (int i = 0; i < random.Next(8, 25); i++)
                        {
                            problem.Tests.Add(new Test
                            {
                                IsTrialTest = random.Next() % 2 == 0 ? true : false,
                                OrderBy = random.Next()
                            });
                        }
                    }

                    foreach (var test in problem.Tests)
                    {
                        test.TestRuns.Add(new TestRun
                                                    {
                                                        MemoryUsed = random.Next(),
                                                        ResultType = (TestRunResultType)random.Next(0, 5),
                                                        TimeUsed = random.Next(),
                                                        CheckerComment = "Checker comment: " + Comments[random.Next(0, 3)],
                                                        ExecutionComment = "Executioner comment: " + Comments[random.Next(0, 3)],
                                                        SubmissionId = submission.Id
                                                    });
                    }

                }

                data.SaveChanges();

                Console.WriteLine("Sleeping for 5s");
                System.Threading.Thread.Sleep(5000);
            }
        }
    }
}
