namespace OJS.Workers.Checkers.Tests
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class CSharpCodeCheckerTests
    {
        [Test]
        public void CallingCheckMethodBeforeSetParameterShouldThrowAnException()
        {
            // Arrange
            var checker = new CSharpCodeChecker();
            string expectedErrorMessage = "Please call SetParameter first with non-null string.";

            // Act
            var exception = Assert.Throws<InvalidOperationException>(() => checker.Check(string.Empty, string.Empty, string.Empty, false));

            // Assert
            Assert.AreEqual(expectedErrorMessage, exception.Message);
        }

        [Test]
        public void SetParameterThrowsExceptionWhenGivenInvalidCode()
        {
            // Arrange
            var checker = new CSharpCodeChecker();

            // Act and Assert
            Assert.Throws<Exception>(() => checker.SetParameter(@"."));
        }

        [Test]
        public void SetParameterThrowsExceptionWhenNotGivenICheckerImplementation()
        {
            // Arragne
            var checker = new CSharpCodeChecker();
            string expectedErrorMessage = "Implementation of OJS.Workers.Common.IChecker not found!";

            // Act
            var exception = Assert.Throws<Exception>(() => checker.SetParameter(@"public class MyChecker { }"));

            // Assert
            Assert.AreEqual(expectedErrorMessage, exception.Message);
        }

        [Test]
        public void SetParameterThrowsExceptionWhenGivenMoreThanOneICheckerImplementation()
        {
            // Arrange
            var checker = new CSharpCodeChecker();
            var SetParameter = @"
                using OJS.Workers.Common;
                public class MyChecker1 : IChecker
                {
                    public CheckerResult Check(string inputData, string receivedOutput, string expectedOutput, bool isTrialTest)
                    {
                        return new CheckerResult
                                    {
                                        IsCorrect = true,
                                        ResultType = CheckerResultType.Ok,
                                        CheckerDetails = new CheckerDetails(),
                                    };
                    }
                    public void SetParameter(string parameter)
                    {
                    }
                }
                public class MyChecker2 : IChecker
                {
                    public CheckerResult Check(string inputData, string receivedOutput, string expectedOutput, bool isTrialTest)
                    {
                        return new CheckerResult
                                    {
                                        IsCorrect = true,
                                        ResultType = CheckerResultType.Ok,
                                        CheckerDetails = new CheckerDetails(),
                                    };
                    }
                    public void SetParameter(string parameter)
                    {
                    }
                }";

            string expectedErrorMessage = "More than one implementation of OJS.Workers.Common.IChecker was found!";

            // Act
            var exception = Assert.Throws<Exception>(() => checker.SetParameter(SetParameter), "Hola", null);

            // Assert
            Assert.AreEqual(expectedErrorMessage, exception.Message);
        }

        [Test]
        public void CheckMethodWorksCorrectlyWithSomeCheckerCode()
        {
            var checker = new CSharpCodeChecker();
            checker.SetParameter(@"
                using OJS.Workers.Common;
                public class MyChecker : IChecker
                {
                    public CheckerResult Check(string inputData, string receivedOutput, string expectedOutput, bool isTrialTest)
                    {
                        return new CheckerResult
                                    {
                                        IsCorrect = true,
                                        ResultType = CheckerResultType.Ok,
                                        CheckerDetails = new CheckerDetails { Comment = ""It was me"" },
                                    };
                    }
                    public void SetParameter(string parameter)
                    {
                    }
                }");

            var checkerResult = checker.Check(string.Empty, string.Empty, string.Empty, false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
            Assert.AreEqual("It was me", checkerResult.CheckerDetails.Comment);
        }

        [Test]
        public void CheckMethodReceivesCorrectParameters()
        {
            var checker = new CSharpCodeChecker();
            checker.SetParameter(@"
                using OJS.Workers.Common;
                public class MyChecker : IChecker
                {
                    public CheckerResult Check(string inputData, string receivedOutput, string expectedOutput, bool isTrialTest)
                    {
                        bool isCorrect = true;

                        if (inputData != ""One"") isCorrect = false;
                        if (receivedOutput != ""Two"") isCorrect = false;
                        if (expectedOutput != ""Three"") isCorrect = false;

                        return new CheckerResult
                                    {
                                        IsCorrect = isCorrect,
                                        ResultType = CheckerResultType.Ok,
                                        CheckerDetails = new CheckerDetails(),
                                    };
                    }
                    public void SetParameter(string parameter)
                    {
                    }
                }");

            var checkerResult = checker.Check("One", "Two", "Three", false);

            Assert.IsNotNull(checkerResult);
            Assert.IsTrue(checkerResult.IsCorrect);
        }
    }
}
