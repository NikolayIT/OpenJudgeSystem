namespace OJS.Data.Models.Tests
{
    using System.Collections.Generic;

    using NUnit.Framework;

    using OJS.Workers.Common.Models;

    [TestFixture]
    public class SubmissionTests
    {
        [Test]
        public void CacheTestRunsShouldSetValidResult()
        {
            var submission = new Submission
            {
                TestRuns = new List<TestRun>
                {
                    new TestRun { Test = new Test { IsTrialTest = true }, Id = 1, ResultType = TestRunResultType.CorrectAnswer },
                    new TestRun { Test = new Test { IsTrialTest = false }, Id = 2, ResultType = TestRunResultType.WrongAnswer },
                    new TestRun { Test = new Test { IsTrialTest = false }, Id = 3, ResultType = TestRunResultType.CorrectAnswer },
                    new TestRun { Test = new Test { IsTrialTest = true }, Id = 4, ResultType = TestRunResultType.TimeLimit },
                    new TestRun { Test = new Test { IsTrialTest = false }, Id = 5, ResultType = TestRunResultType.CorrectAnswer },
                    new TestRun { Test = new Test { IsTrialTest = false }, Id = 6, ResultType = TestRunResultType.MemoryLimit },
                    new TestRun { Test = new Test { IsTrialTest = false }, Id = 7, ResultType = TestRunResultType.CorrectAnswer },
                    new TestRun { Test = new Test { IsTrialTest = true }, Id = 8, ResultType = TestRunResultType.RunTimeError },
                    new TestRun { Test = new Test { IsTrialTest = false }, Id = 9, ResultType = TestRunResultType.CorrectAnswer },
                    new TestRun { Test = new Test { IsTrialTest = false }, Id = 10, ResultType = TestRunResultType.WrongAnswer },
                }
            };

            submission.CacheTestRuns();

            Assert.AreEqual("30241003001", submission.TestRunsCache);
        }
    }
}
