namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Common;
    using OJS.Data.Contracts;

    public class Problem : DeletableEntity, IOrderable
    {
        [Key]
        public int Id { get; set; }

        public int ProblemGroupId { get; set; }

        public virtual ProblemGroup ProblemGroup { get; set; }

        [Required]
        [MaxLength(GlobalConstants.ProblemNameMaxLength)]
        public string Name { get; set; }

        public short MaximumPoints { get; set; }

        /// <summary>
        /// Gets or sets a Time limit for the problem. Measured in milliseconds.
        /// </summary>
        public int TimeLimit { get; set; }

        /// <summary>
        /// Gets or sets a Memory limit for the problem. Measured in bytes.
        /// </summary>
        public int MemoryLimit { get; set; }

        /// <summary>
        /// Gets or sets a File size limit (measured in bytes).
        /// </summary>
        public int? SourceCodeSizeLimit { get; set; }

        public int? CheckerId { get; set; }

        public virtual Checker Checker { get; set; }

        public int OrderBy { get; set; }

        /// <summary>
        /// Gets or sets a predefined skeleton for the task
        /// </summary>
        public byte[] SolutionSkeleton { get; set; }

        /// <summary>
        /// Gets or sets Problem specific dependencies that will be compiled and executed with the user code
        /// such as Solution skeletons, mocks or data and text files.
        /// </summary>
        public byte[] AdditionalFiles { get; set; }

        [DefaultValue(true)]
        [Index]
        public bool ShowResults { get; set; }

        [DefaultValue(false)]
        public bool ShowDetailedFeedback { get; set; }

        public virtual ICollection<Test> Tests { get; set; } = new HashSet<Test>();

        public virtual ICollection<ProblemResource> Resources { get; set; } = new HashSet<ProblemResource>();

        public virtual ICollection<Submission> Submissions { get; set; } = new HashSet<Submission>();

        public virtual ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();

        public virtual ICollection<ParticipantScore> ParticipantScores { get; set; } = new HashSet<ParticipantScore>();

        public virtual ICollection<SubmissionType> SubmissionTypes { get; set; } = new HashSet<SubmissionType>();

        public virtual ICollection<Participant> Participants { get; set; } = new HashSet<Participant>();
    }
}