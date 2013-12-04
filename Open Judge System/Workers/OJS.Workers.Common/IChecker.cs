namespace OJS.Workers.Common
{
    public interface IChecker
    {
        CheckerResult Check(string inputData, string receivedOutput, string expectedOutput, bool isTrialTest);

        void SetParameter(string parameter);
    }
}
