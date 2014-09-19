namespace OJS.Workers.Tools.AntiCheat
{
    public interface IPlagiarismDetector
    {
        PlagiarismResult DetectPlagiarism(string firstSource, string secondSource);
    }
}
