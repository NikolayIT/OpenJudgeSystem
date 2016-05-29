namespace OJS.Workers.Tools.AntiCheat.Contracts
{
    public interface IPlagiarismDetectorFactory
    {
        IPlagiarismDetector CreatePlagiarismDetector(PlagiarismDetectorCreationContext context);
    }
}
