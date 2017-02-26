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
        private ICollection<Test> tests;
        private ICollection<ProblemResource> resources;
        private ICollection<Submission> submissions;
        private ICollection<Tag> tags;
        private ICollection<ParticipantScore> participantScores;

        public Problem()
        {
            this.tests = new HashSet<Test>();
            this.resources = new HashSet<ProblemResource>();
            this.submissions = new HashSet<Submission>();
            this.tags = new HashSet<Tag>();
            this.participantScores = new HashSet<ParticipantScore>();
        }

        [Key]
        public int Id { get; set; }

        public int? OldId { get; set; }

        public int ContestId { get; set; }

        public virtual Contest Contest { get; set; }

        [Required]
        [MaxLength(GlobalConstants.ProblemNameMaxLength)]
        public string Name { get; set; }

        public short MaximumPoints { get; set; }

        /// <summary>
        /// Time limit for the problem. Measured in milliseconds.
        /// </summary>
        public int TimeLimit { get; set; }

        /// <summary>
        /// Memory limit for the problem. Measured in bytes.
        /// </summary>
        public int MemoryLimit { get; set; }

        /// <summary>
        /// File size limit (measured in bytes).
        /// </summary>
        public int? SourceCodeSizeLimit { get; set; }

        [ForeignKey("Checker")]
        public int? CheckerId { get; set; }

        public virtual Checker Checker { get; set; }

        public int OrderBy { get; set; }

        /// <summary>
        /// Predefined skeleton for the task
        /// </summary>
        public byte[] SolutionSkeleton { get; set; }

        [DefaultValue(true)]
        [Index]
        public bool ShowResults { get; set; }

        [DefaultValue(false)]
        public bool ShowDetailedFeedback { get; set; }

        public virtual ICollection<Test> Tests
        {
            get { return this.tests; }
            set { this.tests = value; }
        }

        public virtual ICollection<ProblemResource> Resources
        {
            get { return this.resources; }
            set { this.resources = value; }
        }

        public virtual ICollection<Submission> Submissions
        {
            get { return this.submissions; }
            set { this.submissions = value; }
        }

        public virtual ICollection<Tag> Tags
        {
            get { return this.tags; }
            set { this.tags = value; }
        }

        public virtual ICollection<ParticipantScore> ParticipantScores
        {
            get { return this.participantScores; }
            set { this.participantScores = value; }
        }
    }
}
