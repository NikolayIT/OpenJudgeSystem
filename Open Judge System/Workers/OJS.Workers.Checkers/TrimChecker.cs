namespace OJS.Workers.Checkers
{
    using OJS.Workers.Common;

    public class TrimChecker : Checker
    {
        public override CheckerResult Check(string inputData, string receivedOutput, string expectedOutput, bool isTrialTest)
        {
            var result = this.CheckLineByLine(
                inputData,
                receivedOutput?.Trim(),
                expectedOutput?.Trim(),
                this.AreEqualTrimmedLines,
                isTrialTest);
            return result;
        }
    }
}
