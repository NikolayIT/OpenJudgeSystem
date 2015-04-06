﻿namespace OJS.Web.Areas.Contests.ViewModels.Submissions
{
    using System;
    using System.Linq.Expressions;

    using OJS.Common.Models;
    using OJS.Data.Models;

    public class TestRunDetailsViewModel
    {
        public static Expression<Func<TestRun, TestRunDetailsViewModel>> FromTestRun
        {
            get
            {
                return test => new TestRunDetailsViewModel
                {
                    IsTrialTest = test.Test.IsTrialTest,
                    CheckerComment = test.CheckerComment,
                    ExpectedOutputFragment = test.ExpectedOutputFragment,
                    UserOutputFragment = test.UserOutputFragment,
                    ExecutionComment = test.ExecutionComment,
                    Order = test.Test.OrderBy,
                    ResultType = test.ResultType,
                    TimeUsed = test.TimeUsed,
                    MemoryUsed = test.MemoryUsed,
                    Id = test.Id,
                    TestId = test.TestId,
                };
            }
        }

        public bool IsTrialTest { get; set; }

        public int Order { get; set; }

        public TestRunResultType ResultType { get; set; }

        public string ExecutionComment { get; set; }

        public string CheckerComment { get; set; }

        public string ExpectedOutputFragment { get; set; }

        public string UserOutputFragment { get; set; }

        public int TimeUsed { get; set; }

        public long MemoryUsed { get; set; }

        public int Id { get; set; }

        public int TestId { get; set; }
    }
}