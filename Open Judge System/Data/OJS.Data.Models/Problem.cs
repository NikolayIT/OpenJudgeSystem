namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Data.Contracts;

    public class Problem : DeletableEntity, IOrderable
    {
        private ICollection<Test> tests;
        private ICollection<ProblemResource> resources;
        private ICollection<Submission> submissions;
        private ICollection<Tag> tags;

        public Problem()
        {
            this.tests = new HashSet<Test>();
            this.resources = new HashSet<ProblemResource>();
            this.submissions = new HashSet<Submission>();
            this.tags = new HashSet<Tag>();
        }

        [Key]
        public int Id { get; set; }

        public int? OldId { get; set; }

        public int ContestId { get; set; }

        public virtual Contest Contest { get; set; }

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

        [DefaultValue(true)]
        public bool ShowResults { get; set; }

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
    }
}
