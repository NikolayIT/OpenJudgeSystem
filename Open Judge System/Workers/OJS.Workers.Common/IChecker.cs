namespace OJS.Workers.Common
{
    public interface IChecker
    {
        CheckerResult Check(string inputData, string receivedOutput, string expectedOutput);

        void SetParameter(string parameter);
    }
}
