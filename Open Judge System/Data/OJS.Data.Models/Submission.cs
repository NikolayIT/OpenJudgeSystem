namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data.Contracts;

    public class Submission : DeletableEntity
    {
        private ICollection<TestRun> testRuns;

        public Submission()
        {
            this.testRuns = new HashSet<TestRun>();
        }

        public int Id { get; set; }

        public int? ParticipantId { get; set; }

        public virtual Participant Participant { get; set; }

        public int? ProblemId { get; set; }

        public virtual Problem Problem { get; set; }

        public int? SubmissionTypeId { get; set; }

        public virtual SubmissionType SubmissionType { get; set; }

        /// <remarks>
        /// Using byte[] (compressed with deflate) to save database space.
        /// </remarks>
        public byte[] Content { get; set; }

        [NotMapped]
        public string ContentAsString
        {
            get
            {
                return this.Content.Decompress();
            }

            set
            {
                this.Content = value.Compress();
            }
        }

        public bool IsCompiledSuccessfully { get; set; }

        public string CompilerComment { get; set; }

        public virtual ICollection<TestRun> TestRuns
        {
            get { return this.testRuns; }
            set { this.testRuns = value; }
        }

        public bool Processed { get; set; }

        public bool Processing { get; set; }

        public string ProcessingComment { get; set; }

        /// <summary>
        /// Cache field for submissions points (to speed-up some of the database queries)
        /// </summary>
        public int Points { get; set; }

        [NotMapped]
        public int CorrectTestRunsCount
        {
            get
            {
                return this.TestRuns.Count(x => x.ResultType == TestRunResultType.CorrectAnswer);
            }
        }

        [NotMapped]
        public int CorrectTestRunsWithoutTrialTestsCount
        {
            get
            {
                return this.TestRuns.Count(x => x.ResultType == TestRunResultType.CorrectAnswer && !x.Test.IsTrialTest);
            }
        }

        [NotMapped]
        public int TestsWithoutTrialTestsCount
        {
            get
            {
                return this.Problem.Tests.Where(x => !x.IsTrialTest).Count();
            }
        }
    }
}
