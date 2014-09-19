namespace OJS.Workers.Tools.AntiCheat
{
    using System.Collections.Generic;

    public interface IPlagiarismResult
    {
        byte SimilarityPercentage { get; set; }

        IEnumerable<IPlagiarismResult> Type { get; set; }
    }
}
