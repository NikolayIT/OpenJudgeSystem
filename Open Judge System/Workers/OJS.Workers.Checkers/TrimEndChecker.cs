namespace OJS.Workers.Checkers
{
    using OJS.Workers.Common;

    public class TrimEndChecker : Checker
    {
        public override CheckerResult Check(string inputData, string receivedOutput, string expectedOutput, bool isTrialTest)
        {
            var result = this.CheckLineByLine(inputData, receivedOutput.TrimEnd(), expectedOutput.TrimEnd(), this.AreEqualEndTrimmedLines, isTrialTest);
            return result;
        }
    }
}
