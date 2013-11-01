namespace OJS.Workers.Checkers
{
    using System;
    using System.IO;

    using OJS.Workers.Common;

    public abstract class BaseChecker : IChecker
    {
        public abstract CheckerResult Check(string inputData, string receivedOutput, string expectedOutput);

        public virtual void SetParameter(string parameter)
        {
            throw new InvalidOperationException("This checker doesn't support parameters");
        }

        protected CheckerResult CheckLineByLine(string inputData, string receivedOutput, string expectedOutput, Func<string, string, bool> areEqual)
        {
            this.NormalizeEndLines(ref receivedOutput);
            this.NormalizeEndLines(ref expectedOutput);

            var userFileReader = new StringReader(receivedOutput);
            var correctFileReader = new StringReader(expectedOutput);

            CheckerResultType resultType;

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
                            resultType = CheckerResultType.InvalidNumberOfLines;
                            break;
                        }

                        if (!areEqual(userLine, correctLine))
                        {
                            // Lines are different => wrong answer
                            resultType = CheckerResultType.WrongAnswer;
                            break;
                        }
                    }
                }
            }

            return new CheckerResult
            {
                IsCorrect = resultType == CheckerResultType.Ok,
                ResultType = resultType,
                //// TODO: Include line numbers difference
                CheckerDetails = string.Empty
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
    }
}
