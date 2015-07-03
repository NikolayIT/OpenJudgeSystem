namespace OJS.Workers.ExecutionStrategies.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class MochaExecutionResultTests
    {
        private const string ParseErrorMessage = "Invalid console output!";

        [Test]
        public void MochaExecutionResultParseShouldReturnPassedWithCorrectPassesJSONString()
        {
            var jsonString = "{stats: {passes: 1}}";

            var result = MochaExecutionResult.Parse(jsonString);

            Assert.IsTrue(result.Passed);
            Assert.IsNullOrEmpty(result.Error);
        }

        [Test]
        public void MochaExecutionResultParseShouldReturnNotPassedWithCorrectErrorWithCorrectNoPassesJSONString()
        {
            var jsonString = "{stats: {passes: 0}}";

            var result = MochaExecutionResult.Parse(jsonString);

            Assert.IsFalse(result.Passed);
            Assert.AreEqual(ParseErrorMessage, result.Error);
        }

        [Test]
        public void MochaExecutionResultParseShouldReturnNotPassedWithCorrectErrorWithCorrectNoPassesAndFailiureJSONString()
        {
            var jsonString = "{stats: {passes: 0}, failures: [{ err: { message: \"Custom error\" } }]}";

            var result = MochaExecutionResult.Parse(jsonString);

            Assert.IsFalse(result.Passed);
            Assert.AreEqual("Custom error", result.Error);
        }

        [Test]
        public void MochaExecutionResultParseShouldReturnNotPassedWithCorrectErrorInvalidJSONString()
        {
            var jsonString = "{stats: {passes: 1}";

            var result = MochaExecutionResult.Parse(jsonString);

            Assert.IsFalse(result.Passed);
            Assert.AreEqual(ParseErrorMessage, result.Error);
        }

        [Test]
        public void MochaExecutionResultParseShouldReturnNotPassedWithCorrectErrorInvalidTwoObjectsInJSONString()
        {
            var jsonString = "{stats: {passes: 1}} {stats: {passes: 1}}";

            var result = MochaExecutionResult.Parse(jsonString);

            Assert.IsFalse(result.Passed);
            Assert.AreEqual(ParseErrorMessage, result.Error);
        }

        [Test]
        public void MochaExecutionResultParseShouldReturnNotPassedWithCorrectErrorCommentInJSONString()
        {
            var jsonString = "{stats: {passes: 1}} /* comment */ {stats: {passes: 1}}";

            var result = MochaExecutionResult.Parse(jsonString);

            Assert.IsFalse(result.Passed);
            Assert.AreEqual(ParseErrorMessage, result.Error);
        }

        [Test]
        public void MochaExecutionResultParseShouldReturnNotPassedWithCorrectErrorDoubleCommentInJSONString()
        {
            var jsonString = "{stats: {passes: 1}} //** comment **// {stats: {passes: 1}}";

            var result = MochaExecutionResult.Parse(jsonString);

            Assert.IsFalse(result.Passed);
            Assert.AreEqual(ParseErrorMessage, result.Error);
        }
    }
}
