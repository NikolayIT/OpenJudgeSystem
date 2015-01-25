namespace OJS.Workers.Checkers.Tests
{
    using System.Text;

    using NUnit.Framework;

    using OJS.Workers.Common;

    [TestFixture]
    public class TrimCheckerTests
    {
        [Test]
        public void TrimCheckerShouldReturnTrueWhenGivenExactStrings()
        {
            string receivedOutput = "Ивайло";
            string expectedOutput = "Ивайло";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckerShouldReturnTrueWhenGivenExactStringsWithDifferentNewLineEndings()
        {
            string receivedOutput = "Ивайло\n";
            string expectedOutput = "Ивайло";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckerShouldReturnTrueWhenGivenExactMultiLineStrings()
        {
            string receivedOutput = "Ивайло\nFoo\nBar";
            string expectedOutput = "Ивайло\nFoo\nBar";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckerShouldReturnTrueWhenGivenExactMultiLineStringsWithDifferentNewLineEndings()
        {
            string receivedOutput = "Ивайло\nFoo\nBar";
            string expectedOutput = "Ивайло\nFoo\nBar\n";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckerShouldRespectsTextCasing()
        {
            string receivedOutput = "Ивайло";
            string expectedOutput = "ивайло";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [Test]
        public void TrimCheckerShouldRespectsDecimalSeparators()
        {
            string receivedOutput = "1,1";
            string expectedOutput = "1.1";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [Test]
        public void TrimCheckerShouldReturnFalseWhenGivenDifferentStrings()
        {
            string receivedOutput = "Foo";
            string expectedOutput = "Bar";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [Test]
        public void TrimCheckershouldReturnInvalidNumberOfLinesWhenReceivedOutputHasMoreLines()
        {
            string receivedOutput = "Bar\nFoo";
            string expectedOutput = "Bar";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.InvalidNumberOfLines));
        }

        [Test]
        public void TrimCheckerShouldReturnInvalidNumberOfLinesWhenExpectedOutputHasMoreLines()
        {
            string receivedOutput = "Bar";
            string expectedOutput = "Bar\nFoo";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.InvalidNumberOfLines));
        }

        [Test]
        public void TrimCheckerShouldReturnCorrectAnswerWhenGivenDifferentMultiLineStringsWithSameText()
        {
            string receivedOutput = "Bar\nFoo\n\n";
            string expectedOutput = "Bar\nFoo";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckerShouldReturnCorrectAnswerWhenGivenDifferentMultiLineStringsWithSameTextDifferentBlankLines()
        {
            string receivedOutput = "Bar\nFoo\n\n";
            string expectedOutput = "\n\nBar\nFoo";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckerShouldReturnCorrectAnswerInBiggerSameTextsTest()
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

            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString(), false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckerShouldReturnWrongAnswerInBiggerDifferentTextsTest()
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

            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString(), false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [Test]
        public void TrimCheckerShouldReturnCorrectAnswerInBiggerDifferentNumberOfLinesTest()
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

            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString(), false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.InvalidNumberOfLines));
        }

        [Test]
        public void TrimCheckerShouldReturnCorrectAnswerWhenWhitespaceIsDifferentAtTheEndOfStrings()
        {
            string receivedOutput = "Ивайло ";
            string expectedOutput = "Ивайло   ";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckerShouldReturnCorrectAnswerWhenWhitespaceIsDifferentAtTheStartOfStrings()
        {
            string receivedOutput = "   Ивайло";
            string expectedOutput = "    Ивайло";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckerShouldReturnCorrectAnswerWhenWhitespaceIsDifferentAtTheStartAndEndOfStrings()
        {
            string receivedOutput = "   Ивайло  ";
            string expectedOutput = "    Ивайло    ";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckerShouldReturnCorrectAnswerWhenWhitespaceIsDifferentAtTheStartAndEndOfStringsAndDifferentEndLines()
        {
            string receivedOutput = "   Ивайло  \n";
            string expectedOutput = "    Ивайло    ";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckerShouldReturnCorrectAnswerWhenWhitespaceIsDifferentAtTheStartAndEndOfStringsAndTwoDifferentEndLines()
        {
            string receivedOutput = "   Ивайло     ";
            string expectedOutput = "    Ивайло    \n   \n";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckerShouldReturnCorrectAnswerWhenEveryLineHasDifferentWhiteSpace()
        {
            string receivedOutput = "   Ивайло   \n      Кенов   \n   Е \n     Пич     \n";
            string expectedOutput = "     Ивайло      \n        Кенов     \n     Е   \n       Пич       \n";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckerShouldReturnCorrectAnswerWhenLastLineHasDifferentWhiteSpace()
        {
            string receivedOutput = "   Ивайло   \n      Кенов   \n   Е \n     Пич     \n      ";
            string expectedOutput = "     Ивайло      \n        Кенов     \n     Е   \n       Пич       \n";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckerShouldReturnCorrectAnswerWhenLastExpectedLineHasDifferentWhiteSpace()
        {
            string receivedOutput = "   Ивайло   \n      Кенов   \n   Е \n     Пич     \n      ";
            string expectedOutput = "     Ивайло      \n        Кенов     \n     Е   \n       Пич       \n     ";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckerShouldRespectsTextCasingWithWhiteSpace()
        {
            string receivedOutput = " Николай   ";
            string expectedOutput = "   николай  ";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [Test]
        public void TrimCheckerShouldRespectsDecimalSeparatorsWithSpacing()
        {
            string receivedOutput = "  1,1  ";
            string expectedOutput = "    1.1 ";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [Test]
        public void TrimCheckerShouldReturnFalseWhenGivenDifferentStringsWithSpacing()
        {
            string receivedOutput = "   Foo  ";
            string expectedOutput = " Bar ";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [Test]
        public void TrimCheckerShouldReturnInvalidNumberOfLinesWhenReceivedOutputHasMoreLinesWithSpacing()
        {
            string receivedOutput = "Bar  \n   Foo ";
            string expectedOutput = " Bar       ";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.InvalidNumberOfLines));
        }

        [Test]
        public void TrimCheckerShouldReturnCorrectAnswerWhenGivenDifferentMultiLineStringsWithSameTextWithSpacing()
        {
            string receivedOutput = "Bar   \n     Foo \n   \n  \n \n\n";
            string expectedOutput = "Bar \n Foo   ";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckerShouldReturnCorrectAnswerWhenGivenDifferentMultiLineStringsWithSameTextDifferentBlankLinesTest()
        {
            string receivedOutput = "   Bar   \n   Foo   \n  \n  ";
            string expectedOutput = "\n \n Bar \n Foo";
            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckShouldReturnCorrectAnswerInBiggerSameTextsWithDifferentWhiteSpaceTest()
        {
            StringBuilder receivedOutput = new StringBuilder();

            for (int i = 0; i < 1000; i++)
            {
                receivedOutput.Append(new string(' ', i));
                receivedOutput.AppendLine(i.ToString());
            }

            StringBuilder expectedOutput = new StringBuilder();

            for (int i = 0; i < 1000; i++)
            {
                expectedOutput.Append(i.ToString());
                expectedOutput.AppendLine(new string(' ', 1000 - i));
            }

            var checker = new TrimChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString(), false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void TrimCheckerShouldReturnWrongAnswerInBiggerTextsWithLastLineDifferentTest()
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

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString(), false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }
    }
}
