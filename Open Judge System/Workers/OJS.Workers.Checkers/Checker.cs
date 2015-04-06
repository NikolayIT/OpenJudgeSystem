namespace OJS.Workers.Checkers
{
    using System;
    using System.IO;
    using System.Reflection;

    using OJS.Common.Extensions;
    using OJS.Workers.Common;

    public abstract class Checker : IChecker
    {
        protected bool IgnoreCharCasing { get; set; }

        public static IChecker CreateChecker(string assemblyName, string typeName, string parameter)
        {
            var assembly = Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory + assemblyName);
            var type = assembly.GetType(typeName);
            var checker = (IChecker)Activator.CreateInstance(type);

            if (parameter != null)
            {
                checker.SetParameter(parameter);
            }

            return checker;
        }

        public abstract CheckerResult Check(
            string inputData,
            string receivedOutput,
            string expectedOutput,
            bool isTrialTest);

        public virtual void SetParameter(string parameter)
        {
            throw new InvalidOperationException("This checker doesn't support parameters");
        }

        protected CheckerResult CheckLineByLine(
            string inputData,
            string receivedOutput,
            string expectedOutput,
            Func<string, string, bool> areEqual,
            bool isTrialTest)
        {
            this.NormalizeEndLines(ref receivedOutput);
            this.NormalizeEndLines(ref expectedOutput);

            var userFileReader = new StringReader(receivedOutput);
            var correctFileReader = new StringReader(expectedOutput);

            CheckerResultType resultType;

            var adminCheckerDetails = new CheckerDetails();
            int lineNumber = 0;
            using (userFileReader)
            {
                using (correctFileReader)
                {
                    while (true)
                    {
                        string userLine = userFileReader.ReadLine();
                        string correctLine = correctFileReader.ReadLine();

                        if (userLine == null && correctLine == null)
                        {
                            // No more lines in both streams
                            resultType = CheckerResultType.Ok;
                            break;
                        }

                        if (userLine == null || correctLine == null)
                        {
                            // One of the two streams is already empty
                            adminCheckerDetails = this.PrepareAdminCheckerDetailsForInvalidNumberOfLines(
                                lineNumber,
                                userLine,
                                correctLine);
                            resultType = CheckerResultType.InvalidNumberOfLines;
                            break;
                        }

                        if (!areEqual(userLine, correctLine))
                        {
                            // Lines are different => wrong answer
                            adminCheckerDetails = this.PrepareAdminCheckerDetailsForDifferentLines(
                                lineNumber,
                                correctLine,
                                userLine);
                            resultType = CheckerResultType.WrongAnswer;
                            break;
                        }

                        lineNumber++;
                    }
                }
            }

            var checkerDetails = new CheckerDetails();
            if (resultType != CheckerResultType.Ok)
            {
                checkerDetails = this.PrepareCheckerDetails(receivedOutput, expectedOutput, isTrialTest, adminCheckerDetails);
            }

            return new CheckerResult
            {
                IsCorrect = resultType == CheckerResultType.Ok,
                ResultType = resultType,
                CheckerDetails = checkerDetails
            };
        }

        protected void NormalizeEndLines(ref string output)
        {
            if (!output.EndsWith("\n"))
            {
                output += "\n";
            }
        }

        protected bool AreEqualExactLines(string userLine, string correctLine)
        {
            return userLine.Equals(correctLine, StringComparison.InvariantCulture);
        }

        protected bool AreEqualTrimmedLines(string userLine, string correctLine)
        {
            return userLine.Trim().Equals(correctLine.Trim(), StringComparison.InvariantCulture);
        }

        protected bool AreEqualCaseInsensitiveLines(string userLine, string correctLine)
        {
            return userLine.ToLower().Equals(correctLine.ToLower(), StringComparison.InvariantCulture);
        }

        protected virtual CheckerDetails PrepareAdminCheckerDetailsForDifferentLines(
            int lineNumber,
            string correctLine,
            string userLine)
        {
            const int FragmentMaxLength = 256;

            var adminCheckerDetails = new CheckerDetails
            {
                Comment = string.Format("Line {0} is different.", lineNumber)
            };

            var firstDifferenceIndex = correctLine.GetFirstDifferenceIndexWith(userLine, this.IgnoreCharCasing);

            if (correctLine != null)
            {
                adminCheckerDetails.ExpectedOutputFragment =
                    PrepareOutputFragment(correctLine, firstDifferenceIndex, FragmentMaxLength);
            }

            if (userLine != null)
            {
                adminCheckerDetails.UserOutputFragment =
                    PrepareOutputFragment(userLine, firstDifferenceIndex, FragmentMaxLength);
            }

            return adminCheckerDetails;
        }

        protected virtual CheckerDetails PrepareAdminCheckerDetailsForInvalidNumberOfLines(
            int lineNumber,
            string userLine,
            string correctLine)
        {
            const int FragmentMaxLength = 256;

            var adminCheckerDetails = new CheckerDetails
            {
                Comment = string.Format("Invalid number of lines on line {0}", lineNumber)
            };

            var firstDifferenceIndex = correctLine.GetFirstDifferenceIndexWith(userLine, this.IgnoreCharCasing);

            if (correctLine != null)
            {
                adminCheckerDetails.ExpectedOutputFragment =
                    PrepareOutputFragment(correctLine, firstDifferenceIndex, FragmentMaxLength);
            }

            if (userLine != null)
            {
                adminCheckerDetails.UserOutputFragment =
                    PrepareOutputFragment(userLine, firstDifferenceIndex, FragmentMaxLength);
            }

            return adminCheckerDetails;
        }

        protected virtual CheckerDetails PrepareCheckerDetails(
            string receivedOutput,
            string expectedOutput,
            bool isTrialTest,
            CheckerDetails adminCheckerDetails)
        {
            CheckerDetails checkerDetails;
            if (isTrialTest)
            {
                const int FragmentMaxLength = 4096;

                checkerDetails = new CheckerDetails();

                var firstDifferenceIndex = expectedOutput.GetFirstDifferenceIndexWith(receivedOutput, this.IgnoreCharCasing);

                if (expectedOutput != null)
                {
                    checkerDetails.ExpectedOutputFragment =
                        PrepareOutputFragment(expectedOutput, firstDifferenceIndex, FragmentMaxLength);
                }

                if (receivedOutput != null)
                {
                    checkerDetails.UserOutputFragment =
                        PrepareOutputFragment(receivedOutput, firstDifferenceIndex, FragmentMaxLength);
                }
            }
            else
            {
                // Test report for admins
                checkerDetails = adminCheckerDetails;
            }

            return checkerDetails;
        }

        private static string PrepareOutputFragment(string output, int firstDifferenceIndex, int fragmentMaxLength)
        {
            var fragmentStartIndex = Math.Max(firstDifferenceIndex - (fragmentMaxLength / 2), 0);
            var fragmentEndIndex = Math.Min(firstDifferenceIndex + (fragmentMaxLength / 2), output.Length);

            var fragment = output.GetStringWithEllipsisBetween(fragmentStartIndex, fragmentEndIndex);

            return fragment;
        }
    }
}
