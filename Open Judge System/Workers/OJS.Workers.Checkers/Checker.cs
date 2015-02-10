namespace OJS.Workers.Checkers
{
    using System;
    using System.IO;
    using System.Reflection;

    using OJS.Common.Extensions;
    using OJS.Workers.Common;

    public abstract class Checker : IChecker
    {
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

        public abstract CheckerResult Check(string inputData, string receivedOutput, string expectedOutput, bool isTrialTest);

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

            string adminCheckerDetails = null;
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

            string checkerDetails = null;
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

        protected virtual string PrepareAdminCheckerDetailsForDifferentLines(
            int lineNumber,
            string correctLine,
            string userLine)
        {
            string adminCheckerDetails =
                string.Format(
                    "Line {1} is different.{0}{0}Expected line:{0}{2}{0}{0}User line:{0}{3}{0}",
                    Environment.NewLine,
                    lineNumber,
                    correctLine.MaxLength(256),
                    userLine.MaxLength(256));
            return adminCheckerDetails;
        }

        protected virtual string PrepareAdminCheckerDetailsForInvalidNumberOfLines(
            int lineNumber,
            string userLine,
            string correctLine)
        {
            string adminCheckerDetails = string.Format(
                "Invalid number of lines on line {0}{1}{1}",
                lineNumber,
                Environment.NewLine);
            if (userLine != null)
            {
                adminCheckerDetails += string.Format(
                    "Next user line:{1}{0}{1}",
                    userLine.MaxLength(256),
                    Environment.NewLine);
            }

            if (correctLine != null)
            {
                adminCheckerDetails += string.Format(
                    "Next correct line:{1}{0}{1}",
                    correctLine.MaxLength(256),
                    Environment.NewLine);
            }

            return adminCheckerDetails;
        }

        protected virtual string PrepareCheckerDetails(
            string receivedOutput,
            string expectedOutput,
            bool isTrialTest,
            string adminCheckerDetails)
        {
            string checkerDetails;
            if (isTrialTest)
            {
                // Full test report for user
                checkerDetails = string.Format(
                    "Expected output:{0}{1}{0}Your output:{0}{2}",
                    Environment.NewLine,
                    expectedOutput.MaxLength(8192),
                    receivedOutput.MaxLength(8192));
            }
            else
            {
                // Test report for admins
                checkerDetails = adminCheckerDetails;
            }

            return checkerDetails;
        }
    }
}
