namespace OJS.Workers.Tools.AntiCheat
{
    using System;

    using OJS.Workers.Common.Models;
    using OJS.Workers.Tools.Similarity;

    public class PlagiarismDetectorCreationContext
    {
        public PlagiarismDetectorCreationContext(PlagiarismDetectorType type, ISimilarityFinder similarityFinder)
        {
            if (similarityFinder == null)
            {
                throw new ArgumentNullException(nameof(similarityFinder));
            }

            this.Type = type;
            this.SimilarityFinder = similarityFinder;
        }

        public PlagiarismDetectorType Type { get; set; }

        public string CompilerPath { get; set; }

        public string DisassemblerPath { get; set; }

        public ISimilarityFinder SimilarityFinder { get; set; }
    }
}
