namespace OJS.Workers.Checkers
{
    using OJS.Workers.Common;

    public class ExactChecker : Checker
    {
        public override CheckerResult Check(string inputData, string receivedOutput, string expectedOutput)
        {
            var result = this.CheckLineByLine(inputData, receivedOutput, expectedOutput, this.AreEqualExactLines);
            return result;
        }
    }
}
