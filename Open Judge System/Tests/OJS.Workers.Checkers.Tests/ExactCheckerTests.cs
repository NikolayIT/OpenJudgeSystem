namespace OJS.Workers.Checkers.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Text;
    using OJS.Workers.Common;

    [TestClass]
    public class ExactCheckerTests
    {
        [TestMethod]
        public void ExactCheckerShouldReturnTrueWhenGivenExactStrings()
        {
            string receivedOutput = "Николай";
            string expectedOutput = "Николай";
            var checker = new ExactChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [TestMethod]
        public void ExactCheckerShouldReturnTrueWhenGivenExactStringsWithDifferentNewLineEndings()
        {
            string receivedOutput = "Николай\n";
            string expectedOutput = "Николай";
            var checker = new ExactChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [TestMethod]
        public void ExactCheckerShouldReturnTrueWhenGivenExactMultiLineStrings()
        {
            string receivedOutput = "Николай\nFoo\nBar";
            string expectedOutput = "Николай\nFoo\nBar";
            var checker = new ExactChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [TestMethod]
        public void ExactCheckerShouldReturnTrueWhenGivenExactMultiLineStringsWithDifferentNewLineEndings()
        {
            string receivedOutput = "Николай\nFoo\nBar";
            string expectedOutput = "Николай\nFoo\nBar\n";
            var checker = new ExactChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [TestMethod]
        public void ExactCheckerShouldRespectsTextCasing()
        {
            string receivedOutput = "Николай";
            string expectedOutput = "николай";
            var checker = new ExactChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [TestMethod]
        public void ExactCheckerShouldRespectsDecimalSeparators()
        {
            string receivedOutput = "1,1";
            string expectedOutput = "1.1";
            var checker = new ExactChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [TestMethod]
        public void ExactCheckerShouldReturnFalseWhenGivenDifferentStrings()
        {
            string receivedOutput = "Foo";
            string expectedOutput = "Bar";
            var checker = new ExactChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [TestMethod]
        public void ExactCheckerShouldReturnInvalidNumberOfLinesWhenReceivedOutputHasMoreLines()
        {
            string receivedOutput = "Bar\nFoo";
            string expectedOutput = "Bar";
            var checker = new ExactChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.InvalidNumberOfLines));
        }

        [TestMethod]
        public void ExactCheckerShouldReturnInvalidNumberOfLinesWhenExpectedOutputHasMoreLines()
        {
            string receivedOutput = "Bar";
            string expectedOutput = "Bar\nFoo";
            var checker = new ExactChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.InvalidNumberOfLines));
        }

        [TestMethod]
        public void ExactCheckerShouldReturnInvalidNumberOfLinesWhenGivenDifferentMultiLineStringsWithSameText()
        {
            string receivedOutput = "Bar\nFoo\n\n";
            string expectedOutput = "Bar\nFoo";
            var checker = new ExactChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.InvalidNumberOfLines));
        }

        [TestMethod]
        public void ExactCheckerShouldReturnWrongAnswerWhenGivenDifferentMultiLineStringsWithSameTextDifferentBlankLines()
        {
            string receivedOutput = "Bar\nFoo\n\n";
            string expectedOutput = "\n\nBar\nFoo";
            var checker = new ExactChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [TestMethod]
        public void ExacterCheckShouldReturnCorrectAnswerInBiggerSameTextsTest()
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

            var checker = new ExactChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString());

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [TestMethod]
        public void ExactCheckerShouldReturnWrongAnswerInBiggerDifferentTextsTest()
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

            var checker = new ExactChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString());

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [TestMethod]
        public void ExactCheckerShouldReturnInvalidNumberOfLinesInBiggerDifferentNumberOfLinesTest()
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

            var checker = new ExactChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString());

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.InvalidNumberOfLines));
        }

        [TestMethod]
        public void ExactCheckerShouldReturnWrongAnswerInBiggerTextsWithLastLineDifferentTest()
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

            var checker = new ExactChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString());

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }
    }
}
