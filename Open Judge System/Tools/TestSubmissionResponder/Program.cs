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
            var counter = 0;

            while (true)
            {
                FillInPoints(data);

                var submissions = data.Submissions.AllWithDeleted().Where(x => x.TestRuns.Count == 0);

                var submissionCount = submissions.Count();
                var submissionByIteration = 400;

                if (submissionCount != 0)
                {
                    Console.WriteLine("Found {0} submissions.", submissionCount);
                }

                foreach (var submission in submissions)
                {
                    if (counter % submissionByIteration == 0)
                    {
                        GC.Collect();
                        data.SaveChanges();
                        Console.WriteLine("Saving {0} submissions", submissionByIteration);
                    }

                    counter++;
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
                                                        TimeUsed = random.Next(0, 400),
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

        public static void FillInPoints(OjsData data)
        {
            var submissionsWithoutPoints = data.Submissions.All().Where(x => x.Points == 0).ToArray();
            var submissionPerIteration = 300;
            Console.WriteLine("Total submission without result: {0}",submissionsWithoutPoints.Length);

            for (int i = 0; i < submissionsWithoutPoints.Length; i++)
            {
                var submission = submissionsWithoutPoints[i];
                submission.Points = (int)Math.Round(((double)submission.CorrectTestRunsCount * submission.Problem.MaximumPoints) / submission.Problem.Tests.Count);

                if (i % submissionPerIteration == 0)
                {
                    Console.WriteLine("Saved points for {0} submissions", submissionPerIteration);
                    Console.WriteLine("Processed so far: {0}", i);
                    data.SaveChanges();
                }
            }

            data.SaveChanges();
        }
    }
}
