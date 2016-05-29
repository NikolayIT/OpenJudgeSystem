namespace OJS.Workers.Tools.AntiCheat
{
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Workers.Tools.AntiCheat.Contracts;
    using OJS.Workers.Tools.Similarity;
    using OJS.Workers.Tools.Similarity.Contracts;

    public class PlainTextPlagiarismDetector : IPlagiarismDetector
    {
        private readonly ISimilarityFinder similarityFinder;

        public PlainTextPlagiarismDetector()
            : this(new SimilarityFinder())
        {
        }

        public PlainTextPlagiarismDetector(ISimilarityFinder similarityFinder)
        {
            this.similarityFinder = similarityFinder;
        }

        // TODO: This method is very similar to CSharpCompileDecompilePlagiarismDetector.DetectPlagiarism
        public PlagiarismResult DetectPlagiarism(string firstSource, string secondSource, IEnumerable<IDetectPlagiarismVisitor> visitors = null)
        {
            if (visitors != null)
            {
                foreach (var visitor in visitors)
                {
                    firstSource = visitor.Visit(firstSource);
                    secondSource = visitor.Visit(secondSource);
                }
            }

            var differences = this.similarityFinder.DiffText(firstSource, secondSource, true, true, true);

            var differencesCount = differences.Sum(difference => difference.DeletedA + difference.InsertedB);
            decimal textLength = firstSource.Length + secondSource.Length;

            var percentage = textLength > 0
                ? ((textLength - differencesCount) / textLength) * 100
                : 0;

            return new PlagiarismResult(percentage)
            {
                Differences = differences,
                FirstToCompare = firstSource,
                SecondToCompare = secondSource
            };
        }
    }
}