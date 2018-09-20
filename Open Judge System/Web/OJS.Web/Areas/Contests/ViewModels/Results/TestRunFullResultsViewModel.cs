namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Workers.Common.Models;

    public class TestRunFullResultsViewModel
    {
        public bool IsZeroTest { get; set; }

        public TestRunResultType ResultType { get; set; }

        public static IEnumerable<TestRunFullResultsViewModel> FromCache(string testRunsCache)
        {
            if (string.IsNullOrWhiteSpace(testRunsCache))
            {
                return Enumerable.Empty<TestRunFullResultsViewModel>();
            }

            var trialTestsCount = testRunsCache.First() - '0';

            var trialTests = testRunsCache
                .Skip(1)
                .Take(trialTestsCount)
                .Select(s => new TestRunFullResultsViewModel
                {
                    IsZeroTest = true,
                    ResultType = (TestRunResultType)(s - '0')
                });

            var tests = testRunsCache
                .Skip(1 + trialTestsCount)
                .Select(s => new TestRunFullResultsViewModel
                {
                    IsZeroTest = false,
                    ResultType = (TestRunResultType)(s - '0')
                });

            var result = new List<TestRunFullResultsViewModel>();

            result.AddRange(trialTests);
            result.AddRange(tests);

            return result;
        }
    }
}
