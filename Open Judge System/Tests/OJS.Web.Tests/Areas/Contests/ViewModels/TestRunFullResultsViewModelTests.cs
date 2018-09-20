namespace OJS.Web.Tests.Areas.Contests.ViewModels
{
    using System.Linq;

    using NUnit.Framework;

    using OJS.Web.Areas.Contests.ViewModels.Results;
    using OJS.Workers.Common.Models;

    [TestFixture]
    public class TestRunFullResultsViewModelTests
    {
        [Test]
        public void FromCacheShouldReturnCorrectResult()
        {
            var testRunsCache = "30241003001";
            var result = TestRunFullResultsViewModel.FromCache(testRunsCache).ToList();

            Assert.AreEqual(testRunsCache.Length - 1, result.Count);
            Assert.AreEqual(true, result[0].IsZeroTest);
            Assert.AreEqual(TestRunResultType.CorrectAnswer, result[0].ResultType);
            Assert.AreEqual(true, result[1].IsZeroTest);
            Assert.AreEqual(TestRunResultType.TimeLimit, result[1].ResultType);
            Assert.AreEqual(true, result[2].IsZeroTest);
            Assert.AreEqual(TestRunResultType.RunTimeError, result[2].ResultType);
            Assert.AreEqual(false, result[3].IsZeroTest);
            Assert.AreEqual(TestRunResultType.WrongAnswer, result[3].ResultType);
            Assert.AreEqual(false, result[4].IsZeroTest);
            Assert.AreEqual(TestRunResultType.CorrectAnswer, result[4].ResultType);
            Assert.AreEqual(false, result[5].IsZeroTest);
            Assert.AreEqual(TestRunResultType.CorrectAnswer, result[5].ResultType);
            Assert.AreEqual(false, result[6].IsZeroTest);
            Assert.AreEqual(TestRunResultType.MemoryLimit, result[6].ResultType);
            Assert.AreEqual(false, result[7].IsZeroTest);
            Assert.AreEqual(TestRunResultType.CorrectAnswer, result[7].ResultType);
            Assert.AreEqual(false, result[8].IsZeroTest);
            Assert.AreEqual(TestRunResultType.CorrectAnswer, result[8].ResultType);
            Assert.AreEqual(false, result[9].IsZeroTest);
            Assert.AreEqual(TestRunResultType.WrongAnswer, result[9].ResultType);
        }
    }
}
