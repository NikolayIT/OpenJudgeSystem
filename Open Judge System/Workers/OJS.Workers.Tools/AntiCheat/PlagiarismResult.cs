namespace OJS.Workers.Tools.AntiCheat
{
    using System.Collections.Generic;

    using OJS.Workers.Tools.Similarity;

    public class PlagiarismResult
    {
        public byte SimilarityPercentage { get; set; }

        public string FirstToCompare { get; set; }

        public string SecondToCompare { get; set; }

        public IEnumerable<Difference> Differences { get; set; }
    }
}
