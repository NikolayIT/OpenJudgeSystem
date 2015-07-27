namespace OJS.Workers.Checkers.Tests
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class CSharpCodeCheckerTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CallingCheckMethodBeforeSetParameterShouldThrowAnException()
        {
            var checker = new CSharpCodeChecker();
            checker.Check(string.Empty, string.Empty, string.Empty, false);
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void SetParameterThrowsExceptionWhenGivenInvalidCode()
        {
            var checker = new CSharpCodeChecker();
            checker.SetParameter(@".");
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void SetParameterThrowsExceptionWhenNotGivenICheckerImplementation()
        {
            var checker = new CSharpCodeChecker();
            checker.SetParameter(@"public class MyChecker { }");
        }

        [Test]
        [ExpectedException(typeof(Exception))]
        public void SetParameterThrowsExceptionWhenGivenMoreThanOneICheckerImplementation()
        {
            var checker = new CSharpCodeChecker();
            checker.SetParameter(@"
                using OJS.Workers.Common;
                public class MyChecker1 : IChecker
                {
                    public CheckerResult Check(string inputData, string receivedOutput, string expectedOutput)
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
                    public CheckerResult Check(string inputData, string receivedOutput, string expectedOutput)
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
                }");
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
