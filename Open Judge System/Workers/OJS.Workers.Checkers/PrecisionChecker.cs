namespace OJS.Workers.Checkers
{
    using System;
    using System.Globalization;

    using OJS.Workers.Common;

    /// <summary>
    /// Checks if each line of decimals are equal with certain precision (default is 14).
    /// </summary>
    public class PrecisionChecker : Checker
    {
        private int precision = 14;

        public override CheckerResult Check(string inputData, string receivedOutput, string expectedOutput)
        {
            var result = this.CheckLineByLine(inputData, receivedOutput, expectedOutput, this.AreEqualWithPrecision);
            return result;
        }

        public override void SetParameter(string parameter)
        {
            this.precision = int.Parse(parameter, CultureInfo.InvariantCulture);
        }

        private bool AreEqualWithPrecision(string userLine, string correctLine)
        {
            try
            {
                userLine = userLine.Replace(',', '.');
                correctLine = correctLine.Replace(',', '.');
                decimal userLineInNumber = decimal.Parse(userLine, CultureInfo.InvariantCulture);
                decimal correctLineInNumber = decimal.Parse(correctLine, CultureInfo.InvariantCulture);

                // TODO: Change with 1.0 / math.pow(10, xxx)
                decimal precisionEpsilon = 1.0m / (decimal)Math.Pow(10, precision);

                return Math.Abs(userLineInNumber - correctLineInNumber) < precisionEpsilon;
            }
            catch
            {
                return false;
            }
        }
    }
}
