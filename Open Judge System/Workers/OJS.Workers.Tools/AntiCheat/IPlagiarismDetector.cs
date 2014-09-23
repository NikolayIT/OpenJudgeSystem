namespace OJS.Workers.Tools.AntiCheat
{
    using System.Collections.Generic;

    public interface IPlagiarismDetector
    {
        PlagiarismResult DetectPlagiarism(string firstSource, string secondSource, IEnumerable<IDetectPlagiarismVisitor> visitors = null);
    }
}
