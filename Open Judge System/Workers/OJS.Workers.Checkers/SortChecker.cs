namespace OJS.Workers.Checkers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using OJS.Workers.Common;

    public class SortChecker : BaseChecker
    {
        public override CheckerResult Check(string inputData, string receivedOutput, string expectedOutput)
        {
            this.NormalizeEndLines(ref receivedOutput);
            this.NormalizeEndLines(ref expectedOutput);

            var userFileReader = new StringReader(receivedOutput);
            var correctFileReader = new StringReader(expectedOutput);

            var userLines = new List<string>();
            var correctLines = new List<string>();

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
                    }

                    if (userLine != correctLine)
                    {
                        // one of the streams still has lines
                        return new CheckerResult
                        {
                            IsCorrect = false,
                            ResultType = CheckerResultType.InvalidNumberOfLines,
                            //// TODO: Include line numbers difference
                            CheckerDetails = string.Empty
                        };
                    }
                }
            }

            userLines.Sort();
            correctLines.Sort();

            var resultType = CheckerResultType.Ok;

            if (userLines.Where((t, i) => !t.Equals(correctLines[i], StringComparison.InvariantCulture)).Any())
            {
                resultType = CheckerResultType.WrongAnswer;
            }

            return new CheckerResult
            {
                IsCorrect = resultType == CheckerResultType.Ok,
                ResultType = resultType,
                //// TODO: Include line numbers difference
                CheckerDetails = string.Empty
            };
        }
    }
}
