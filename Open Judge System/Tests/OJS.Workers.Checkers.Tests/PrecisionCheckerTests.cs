namespace OJS.Workers.Checkers.Tests
{
    using System.Globalization;
    using System.Text;

    using NUnit.Framework;

    using OJS.Workers.Common;

    [TestFixture]
    public class PrecisionCheckerTests
    {
        [Test]
        public void PrecisionCheckerShouldReturnTrueWhenGivenExactDecimalWithDefaultPrecision()
        {
            string receivedOutput = "1.11111111111111111111111111";
            string expectedOutput = "1.11111111111111111111111112";
            var checker = new PrecisionChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void PrecisionCheckerShouldReturnTrueWhenGivenExactDecimalWithPrecisionOfFive()
        {
            string receivedOutput = "1.000004";
            string expectedOutput = "1.000003";
            var checker = new PrecisionChecker();
            checker.SetParameter("5");

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void PrecisionCheckerShouldReturnWrongAnswerWhenGivenExactDecimalWithPrecisionOfSixAndDifferentDigitsBeforeTheSixthOne()
        {
            string receivedOutput = "1.000004";
            string expectedOutput = "1.000003";
            var checker = new PrecisionChecker();
            checker.SetParameter("6");

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [Test]
        public void PrecisionCheckerShouldReturnFalseIfNoNumberIsEntered()
        {
            string receivedOutput = "Foobar";
            string expectedOutput = "1.000003";
            var checker = new PrecisionChecker();
            checker.SetParameter("6");

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [Test]
        public void PrecisionCheckerShouldReturnCorrectIfTheAnswerRoundUp()
        {
            string receivedOutput = "1.00";
            string expectedOutput = "1.009";
            var checker = new PrecisionChecker();
            checker.SetParameter("2");

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void PrecisionCheckerShouldReturnTrueIfTheAnswerRoundUpClose()
        {
            string receivedOutput = "1.00";
            string expectedOutput = "0.999999";
            var checker = new PrecisionChecker();
            checker.SetParameter("2");

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void PrecisionCheckerShouldReturnTrueIfTheAnswersAreSame()
        {
            string receivedOutput = "0.999999";
            string expectedOutput = "0.999999";
            var checker = new PrecisionChecker();
            checker.SetParameter("2");

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void PrecisionCheckerShouldReturnFalseIfTheAnswersAreDifferent()
        {
            string receivedOutput = "0.123456";
            string expectedOutput = "0.999999";
            var checker = new PrecisionChecker();
            checker.SetParameter("2");

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [Test]
        public void PrecisionCheckerShouldReturnTrueIfTheAnswersAreSameAndPrecisionIsBiggerThanTheNumbers()
        {
            string receivedOutput = "0.999999";
            string expectedOutput = "0.999999";
            var checker = new PrecisionChecker();
            checker.SetParameter("15");

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void PrecisionCheckerShouldReturnTrueIfTheAnswersAreHugeNumbersSameAndLowPrecision()
        {
            string receivedOutput = "1234567.99";
            string expectedOutput = "1234567.9";
            var checker = new PrecisionChecker();
            checker.SetParameter("1");

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void PrecisionCheckerShouldReturnTrueIfAllLinesAreCorrect()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("bg-BG");

            string receivedOutput = "1.00\n15.89955\n9999.87";
            string expectedOutput = "0.999999\n15.8995555\n9999.87000002";
            var checker = new PrecisionChecker();
            checker.SetParameter("4");

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void PrecisionCheckerShouldReturnTrueIfAllLinesAreCorrectWithOneExtraEmptyLine()
        {
            string receivedOutput = "1.00\n15.89955\n9999.87";
            string expectedOutput = "0.999999\n15.8995555\n9999.87000002\n";
            var checker = new PrecisionChecker();
            checker.SetParameter("4");

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void PrecisionCheckerShouldNotRespectsDecimalSeparators()
        {
            string receivedOutput = "1,1";
            string expectedOutput = "1.1";
            var checker = new PrecisionChecker();

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void PrecisionCheckerShouldReturnCorrectAnswerIfCultureIsBulgarian()
        {
            string receivedOutput = "1,00\n15,89955\n9999,87\n";
            string expectedOutput = "1.00\n15.89955\n9999.87";
            var checker = new PrecisionChecker();
            checker.SetParameter("4");

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void PrecisionCheckerShouldReturnInvalidLineNumberIfAllLinesAreCorrectWithOneExtraFullLineOnReceivedOutput()
        {
            string receivedOutput = "1.00\n15.89955\n9999.87\n99.56";
            string expectedOutput = "1.00\n15.89955\n9999.87";
            var checker = new PrecisionChecker();
            checker.SetParameter("4");

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.InvalidNumberOfLines));
        }

        [Test]
        public void PrecisionCheckerShouldReturnInvalidLineNumberIfAllLinesAreCorrectWithOneExtraFullLineOnExpectedOutput()
        {
            string receivedOutput = "1.00\n15.89955\n9999.87";
            string expectedOutput = "1.00\n15.89955\n9999.87\n99.56";
            var checker = new PrecisionChecker();
            checker.SetParameter("4");

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.InvalidNumberOfLines));
        }

        [Test]
        public void PrecisionCheckerShouldReturnInvalidLineNumberIfAllLinesAreCorrectWithALotOfExtraLine()
        {
            string receivedOutput = "1.00\n15.89955\n9999.87";
            string expectedOutput = "1.00\n15.89955\n9999.87\n\n";
            var checker = new PrecisionChecker();
            checker.SetParameter("4");

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.InvalidNumberOfLines));
        }

        [Test]
        public void PrecisionCheckerShouldReturnWrongAnsweIfAllLinesAreCorrectWithALotOfExtraLinesAtTheBeginningAndEnd()
        {
            string receivedOutput = "\n\n1.00\n15.89955\n9999.87";
            string expectedOutput = "1.00\n15.89955\n9999.87\n\n";
            var checker = new PrecisionChecker();
            checker.SetParameter("4");

            var checkerResult = checker.Check(string.Empty, receivedOutput, expectedOutput, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [Test]
        public void PrecisionCheckShouldReturnCorrectAnswerInBiggerSameTextsTest()
        {
            var receivedOutput = new StringBuilder();

            for (decimal i = 0.000001m; i < 1; i += 0.000001m)
            {
                receivedOutput.AppendLine(i.ToString(CultureInfo.InvariantCulture));
            }

            var expectedOutput = new StringBuilder();

            for (decimal i = 0.000001m; i < 1; i += 0.000001m)
            {
                expectedOutput.AppendLine(i.ToString(CultureInfo.InvariantCulture));
            }

            var checker = new PrecisionChecker();
            checker.SetParameter("4");

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString(), false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }

        [Test]
        public void PrecisionCheckShouldReturnWrongAnswerInBiggerSameTextsWithOneDifferentLineTest()
        {
            var receivedOutput = new StringBuilder();
            for (decimal i = 0.000001m; i < 1; i += 0.000001m)
            {
                receivedOutput.AppendLine(i.ToString(CultureInfo.InvariantCulture));
            }

            receivedOutput.AppendFormat("{0}", 5.5554m);

            var expectedOutput = new StringBuilder();
            for (decimal i = 0.000001m; i < 1; i += 0.000001m)
            {
                expectedOutput.AppendLine(i.ToString(CultureInfo.InvariantCulture));
            }

            expectedOutput.AppendFormat("{0}", 5.5553m);

            var checker = new PrecisionChecker();
            checker.SetParameter("4");

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString(), false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [Test]
        public void PrecisionCheckShouldReturnWrongAnswerInBiggerSameTextsWithTotallyDifferentLinesTest()
        {
            var receivedOutput = new StringBuilder();
            for (decimal i = 0.000001m; i < 1; i += 0.000001m)
            {
                receivedOutput.AppendLine(i.ToString(CultureInfo.InvariantCulture));
            }

            receivedOutput.AppendFormat("{0}", 5.5554m);

            var expectedOutput = new StringBuilder();
            for (decimal i = 0.000001m; i < 1; i += 0.000002m)
            {
                expectedOutput.AppendLine(i.ToString(CultureInfo.InvariantCulture));
            }

            expectedOutput.AppendFormat("{0}", 5.5553m);

            var checker = new PrecisionChecker();
            checker.SetParameter("4");

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString(), false);

            Assert.IsNotNull(checkerResult);
            Assert.IsFalse(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.WrongAnswer));
        }

        [Test]
        public void PrecisionCheckShouldReturnCorrectAnswerInBiggerSameTextsWithDifferentPrecisionTest()
        {
            var receivedOutput = new StringBuilder();
            for (decimal i = 0.000001m; i < 1; i += 0.000001m)
            {
                receivedOutput.AppendLine(i.ToString(CultureInfo.InvariantCulture));
            }

            var expectedOutput = new StringBuilder();
            for (decimal i = 0.000001m; i < 1; i += 0.000001m)
            {
                expectedOutput.AppendLine((i + 0.0000001m).ToString(CultureInfo.InvariantCulture));
            }

            var checker = new PrecisionChecker();
            checker.SetParameter("4");

            var checkerResult = checker.Check(string.Empty, receivedOutput.ToString(), expectedOutput.ToString(), false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.IsTrue(checkerResult.ResultType.HasFlag(CheckerResultType.Ok));
        }
    }
}
