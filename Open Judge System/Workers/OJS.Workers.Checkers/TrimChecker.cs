namespace OJS.Workers.Checkers
{
    using OJS.Workers.Common;

    public class TrimChecker : Checker
    {
        public override CheckerResult Check(string inputData, string receivedOutput, string expectedOutput, bool isTrialTest)
        {
            var result = this.CheckLineByLine(
                inputData,
                receivedOutput == null ? null : receivedOutput.Trim(),
                expectedOutput == null ? null : expectedOutput.Trim(),
                this.AreEqualTrimmedLines,
                isTrialTest);
            return result;
        }
    }
}
