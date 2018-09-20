namespace OJS.Web.ViewModels.TestRun
{
    using System;
    using System.Linq.Expressions;

    using OJS.Data.Models;
    using OJS.Workers.Common.Models;

    public class TestRunViewModel
    {
        public static Expression<Func<TestRun, TestRunViewModel>> FromTestRun
        {
            get
            {
                return testRun => new TestRunViewModel
                {
                    Id = testRun.Id,
                    ExecutionResult = testRun.ResultType,
                    MemoryUsed = testRun.MemoryUsed,
                    TimeUsed = testRun.TimeUsed,
                    ExecutionComment = testRun.ExecutionComment,
                    IsTrialTest = testRun.Test.IsTrialTest
                };
            }
        }

        public int Id { get; set; }

        public int TimeUsed { get; set; }

        public long MemoryUsed { get; set; }

        public TestRunResultType ExecutionResult { get; set; }

        public string ExecutionComment { get; set; }

        public bool IsTrialTest { get; set; }
    }
}