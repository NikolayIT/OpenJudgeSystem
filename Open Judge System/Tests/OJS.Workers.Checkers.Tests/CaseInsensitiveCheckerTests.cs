namespace OJS.Workers.Checkers.Tests
{
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    using OJS.Workers.Common;

    [TestClass]
    public class CaseInsensitiveCheckerTests
    {
        [TestMethod]
        public void CaseInsensitiveCheckerShouldReturnTrueWhenGivenCaseInsensitiveStrings()
        {
            string receivedOutput = "НиколАй";
            string expectedOutput = "НикОлай";
            var checker = new CaseInsensitiveChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [TestMethod]
        public void CaseInsensitiveCheckerShouldReturnTrueWhenGivenCaseInsensitiveStringsWithDifferentNewLineEndings()
        {
            string receivedOutput = "НикоЛай\n";
            string expectedOutput = "ниКолай";
            var checker = new CaseInsensitiveChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [TestMethod]
        public void CaseInsensitiveCheckerShouldReturnTrueWhenGivenCaseInsensitiveMultiLineStrings()
        {
            string receivedOutput = "НикОлай\nFoo\nBAr";
            string expectedOutput = "николай\nFoO\nBar";
            var checker = new CaseInsensitiveChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [TestMethod]
        public void CaseInsensitiveCheckerShouldReturnTrueWhenGivenCaseInsensitiveMultiLineStringsWithDifferentNewLineEndings()
        {
            string receivedOutput = "Николай\nFOo\nBar";
            string expectedOutput = "Николай\nFoo\nBAr\n";
            var checker = new CaseInsensitiveChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [TestMethod]
        public void CaseInsensitiveCheckerShouldNotRespectsTextCasing()
        {
            string receivedOutput = "Николай";
            string expectedOutput = "николай";
            var checker = new CaseInsensitiveChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [TestMethod]
        public void CaseInsensitiveCheckerShouldRespectsDecimalSeparators()
        {
            string receivedOutput = "1,1";
            string expectedOutput = "1.1";
            var checker = new CaseInsensitiveChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [TestMethod]
        public void CaseInsensitiveCheckerShouldReturnFalseWhenGivenDifferentStrings()
        {
            string receivedOutput = "Foo";
            string expectedOutput = "Bar";
            var checker = new CaseInsensitiveChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [TestMethod]
        public void CaseInsensitiveCheckerShouldReturnInvalidNumberOfLinesWhenReceivedOutputHasMoreLines()
        {
            string receivedOutput = "Bar\nFoo";
            string expectedOutput = "Bar";
            var checker = new CaseInsensitiveChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.InvalidNumberOfLines));
        }

        [TestMethod]
        public void CaseInsensitiveCheckerShouldReturnInvalidNumberOfLinesWhenExpectedOutputHasMoreLines()
        {
            string receivedOutput = "Bar";
            string expectedOutput = "Bar\nFoo";
            var checker = new CaseInsensitiveChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.InvalidNumberOfLines));
        }

        [TestMethod]
        public void CaseInsensitiveCheckerShouldReturnInvalidNumberOfLinesWhenGivenDifferentMultiLineStringsWithSameText()
        {
            string receivedOutput = "Bar\nFoo\n\n";
            string expectedOutput = "Bar\nFoo";
            var checker = new CaseInsensitiveChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.InvalidNumberOfLines));
        }

        [TestMethod]
        public void CaseInsensitiveCheckerShouldReturnWrongAnswerWhenGivenDifferentMultiLineStringsWithSameTextDifferentBlankLines()
        {
            string receivedOutput = "BAr\nFoo\n\n";
            string expectedOutput = "\n\nBar\nFOo";
            var checker = new CaseInsensitiveChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [TestMethod]
        public void CaseInsensitiveCheckShouldReturnCorrectAnswerInBiggerSameTextsTest()
        {
            StringBuilder receivedOutput = new StringBuilder();

            for (int i = 0; i < 100000; i++)
            {
                receivedOutput.AppendLine(i.ToString());
            }

            StringBuilder expectedOutput = new StringBuilder();

            for (int i = 0; i < 100000; i++)
            {
                expectedOutput.AppendLine(i.ToString());
            }

            var checker = new CaseInsensitiveChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString());

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [TestMethod]
        public void CaseInsensitiveCheckerShouldReturnWrongAnswerInBiggerDifferentTextsTest()
        {
            StringBuilder receivedOutput = new StringBuilder();

            for (int i = 0; i < 100000; i++)
            {
                receivedOutput.AppendLine(i.ToString());
            }

            StringBuilder expectedOutput = new StringBuilder();

            for (int i = 100000; i > 0; i--)
            {
                expectedOutput.AppendLine(i.ToString());
            }

            var checker = new CaseInsensitiveChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString());

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [TestMethod]
        public void CaseInsensitiveCheckerShouldReturnInvalidNumberOfLinesInBiggerDifferentNumberOfLinesTest()
        {
            StringBuilder receivedOutput = new StringBuilder();

            for (int i = 0; i < 10000; i++)
            {
                receivedOutput.AppendLine(i.ToString());
            }

            StringBuilder expectedOutput = new StringBuilder();

            for (int i = 0; i < 100000; i++)
            {
                expectedOutput.AppendLine(i.ToString());
            }

            var checker = new CaseInsensitiveChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString());

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.InvalidNumberOfLines));
        }

        [TestMethod]
        public void CaseInsensitiveCheckerShouldReturnWrongAnswerInBiggerTextsWithLastLineDifferentTest()
        {
            StringBuilder receivedOutput = new StringBuilder();

            for (int i = 0; i < 100000; i++)
            {
                receivedOutput.AppendLine(i.ToString());
            }

            receivedOutput.Append(1);

            StringBuilder expectedOutput = new StringBuilder();

            for (int i = 0; i < 100000; i++)
            {
                expectedOutput.AppendLine(i.ToString());
            }

            expectedOutput.Append(2);

            var checker = new CaseInsensitiveChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString());

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [TestMethod]
        public void CaseInsensitiveCheckShouldReturnCorrectAnswerInBiggerSameTextsDifferentCaseTest()
        {
            StringBuilder receivedOutput = new StringBuilder();

            for (int i = 0; i < 128; i++)
            {
                receivedOutput.AppendLine(((char)i).ToString().ToLower());
            }

            StringBuilder expectedOutput = new StringBuilder();

            for (int i = 0; i < 128; i++)
            {
                expectedOutput.AppendLine(((char)i).ToString().ToUpper());
            }

            var checker = new CaseInsensitiveChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString());

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }
    }
}
