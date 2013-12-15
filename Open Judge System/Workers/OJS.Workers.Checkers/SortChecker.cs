namespace OJS.Workers.Checkers
{
    using System.Collections.Generic;
    using System.IO;

    using OJS.Workers.Common;

    public class SortChecker : Checker
    {
        public override CheckerResult Check(string inputData, string receivedOutput, string expectedOutput, bool isTrialTest)
        {
            this.NormalizeEndLines(ref receivedOutput);
            this.NormalizeEndLines(ref expectedOutput);

            var userFileReader = new StringReader(receivedOutput);
            var correctFileReader = new StringReader(expectedOutput);

            var userLines = new List<string>();
            var correctLines = new List<string>();

            var resultType = CheckerResultType.Ok;

            string adminCheckerDetails = null;
            int lineNumber = 0;
            using (userFileReader)
            {
                using (correctFileReader)
                {
                    var userLine = userFileReader.ReadLine();
                    var correctLine = correctFileReader.ReadLine();

                    while (userLine != null && correctLine != null)
                    {
                        correctLines.Add(correctLine);
                        userLines.Add(userLine);

                        correctLine = correctFileReader.ReadLine();
                        userLine = userFileReader.ReadLine();

                        lineNumber++;
                    }

                    if (userLine != correctLine)
                    {
                        // one of the streams still has lines
                        adminCheckerDetails = this.PrepareAdminCheckerDetailsForInvalidNumberOfLines(lineNumber, userLine, correctLine);
                        resultType = CheckerResultType.InvalidNumberOfLines;
                    }
                }
            }

            userLines.Sort();
            correctLines.Sort();

            if (resultType == CheckerResultType.Ok)
            {
                for (int i = 0; i < userLines.Count; i++)
                {
                    if (!this.AreEqualExactLines(userLines[i], correctLines[i]))
                    {
                        adminCheckerDetails = this.PrepareAdminCheckerDetailsForDifferentLines(
                            i,
                            correctLines[i],
                            userLines[i]);
                        resultType = CheckerResultType.WrongAnswer;
                        break;
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
    }
}
