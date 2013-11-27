namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System;
    using System.Linq.Expressions;

    using OJS.Common.Models;
    using OJS.Data.Models;

    public class TestRunFullResultsViewModel
    {
        public static Expression<Func<TestRun, TestRunFullResultsViewModel>> FromTestRun
        {
            get
            {
                return testRun => new TestRunFullResultsViewModel
                {
                    TestRunId = testRun.Id,
                    IsZeroTest = testRun.Test.IsTrialTest,
                    TestId = testRun.TestId,
                    ResultType = testRun.ResultType,
                };
            }
        }

        public int TestRunId { get; set; }

        public bool IsZeroTest { get; set; }

        public int TestId { get; set; }

        public TestRunResultType ResultType { get; set; }
    }
}
