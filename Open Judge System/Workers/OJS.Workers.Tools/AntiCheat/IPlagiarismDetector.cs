namespace OJS.Workers.Tools.AntiCheat
{
    public interface IPlagiarismDetector
    {
        IPlagiarismResult DetectPlagiarism(string firstSource, string secondSource);
    }
}
