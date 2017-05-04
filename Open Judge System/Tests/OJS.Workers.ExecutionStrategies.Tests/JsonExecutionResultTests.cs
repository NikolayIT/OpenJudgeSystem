namespace OJS.Workers.ExecutionStrategies.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class JsonExecutionResultTests
    {
        private const string ParseErrorMessage = "Invalid console output!";

        [Test]
        public void MochaExecutionResultParseShouldReturnPassedWithCorrectPassesJSONString()
        {
            var jsonString = "{stats: {passes: 1}}";

            var result = JsonExecutionResult.Parse(jsonString);

            Assert.IsTrue(result.Passed);
            Assert.IsNullOrEmpty(result.Error);
        }

        [Test]
        public void MochaExecutionResultParseShouldReturnNotPassedWithCorrectErrorWithCorrectNoPassesJSONString()
        {
            var jsonString = "{stats: {passes: 0}}";

            var result = JsonExecutionResult.Parse(jsonString);

            Assert.IsFalse(result.Passed);
            Assert.AreEqual(ParseErrorMessage, result.Error);
        }

        [Test]
        public void MochaExecutionResultParseShouldReturnNotPassedWithCorrectErrorWithCorrectNoPassesAndFailiureJSONString()
        {
            var jsonString = "{stats: {passes: 0}, failures: [{ err: { message: \"Custom error\" } }]}";

            var result = JsonExecutionResult.Parse(jsonString);

            Assert.IsFalse(result.Passed);
            Assert.AreEqual("Custom error", result.Error);
        }

        [Test]
        public void MochaExecutionResultParseShouldReturnNotPassedWithCorrectErrorInvalidJSONString()
        {
            var jsonString = "{stats: {passes: 1}";

            var result = JsonExecutionResult.Parse(jsonString);

            Assert.IsFalse(result.Passed);
            Assert.AreEqual(ParseErrorMessage, result.Error);
        }

        [Test]
        public void MochaExecutionResultParseShouldReturnNotPassedWithCorrectErrorInvalidTwoObjectsInJSONString()
        {
            var jsonString = "{stats: {passes: 1}} {stats: {passes: 1}}";

            var result = JsonExecutionResult.Parse(jsonString);

            Assert.IsFalse(result.Passed);
            Assert.AreEqual(ParseErrorMessage, result.Error);
        }

        [Test]
        public void MochaExecutionResultParseShouldReturnNotPassedWithCorrectErrorCommentInJSONString()
        {
            var jsonString = "{stats: {passes: 1}} /* comment */ {stats: {passes: 1}}";

            var result = JsonExecutionResult.Parse(jsonString);

            Assert.IsFalse(result.Passed);
            Assert.AreEqual(ParseErrorMessage, result.Error);
        }

        [Test]
        public void MochaExecutionResultParseShouldReturnNotPassedWithCorrectErrorDoubleCommentInJSONString()
        {
            var jsonString = "{stats: {passes: 1}} //** comment **// {stats: {passes: 1}}";

            var result = JsonExecutionResult.Parse(jsonString);

            Assert.IsFalse(result.Passed);
            Assert.AreEqual(ParseErrorMessage, result.Error);
        }

        [Test]
        public void MochaExecutionResult_WithUnitTests_ShouldReturnCorrectUserTestsCount()
        {
            var jsonString = @"{stats: {passes: 1, tests: 1}, tests: [{fullTitle: ""Test 0 "", err: {}}]}";

            var result = JsonExecutionResult.Parse(jsonString);

            Assert.AreEqual(1, result.UsersTestCount, "Json Execution Result does not return correct number of UserTests.");
        }

        [Test]
        public void MochaExecutionResult_WithUnitTests_ShouldReturnCorrect()
        {
            var jsonString = @"{stats: {passes: 1, tests: 4}, tests: [{fullTitle: ""Test 0 "", err: {}}, {fullTitle: ""Test 1 "", err: {}}, {fullTitle: ""Test 1 "", err: { stack: ""Some Error""}}, {fullTitle: ""Test 1 "", err: { stack: ""Some Other error""}}]}";

            var result = JsonExecutionResult.Parse(jsonString);

            Assert.AreEqual(1, result.InitialPassingTests, "Json Execution Result does not return the correct number of initial passing tests.");
        }
    }
}
