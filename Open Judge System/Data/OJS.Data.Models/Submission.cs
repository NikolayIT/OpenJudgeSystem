namespace OJS.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
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

        [Key]
        public int Id { get; set; }

        public int? ParticipantId { get; set; }

        public virtual Participant Participant { get; set; }

        public int? ProblemId { get; set; }

        public virtual Problem Problem { get; set; }

        public int? SubmissionTypeId { get; set; }

        public virtual SubmissionType SubmissionType { get; set; }

        /// <remarks>
        /// Using byte[] (compressed with deflate) to save database space for text inputs. For other file types the actual file content is saved in the field.
        /// </remarks>
        public byte[] Content { get; set; }

        /// <remarks>
        /// If the value of FileExtension is null, then compressed text file is written in Content
        /// </remarks>
        public string FileExtension { get; set; }

        [StringLength(45)]
        [Column(TypeName = "varchar")]
        public string IpAddress { get; set; }

        [NotMapped]
        public bool IsBinaryFile
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.FileExtension);
            }
        }

        [NotMapped]
        public string ContentAsString
        {
            get
            {
                if (this.IsBinaryFile)
                {
                    throw new InvalidOperationException("This is a binary file (not a text submission).");
                }

                return this.Content.Decompress();
            }

            set
            {
                if (this.IsBinaryFile)
                {
                    throw new InvalidOperationException("This is a binary file (not a text submission).");
                }

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
                return this.Problem.Tests.Count(x => !x.IsTrialTest);
            }
        }
    }
}
