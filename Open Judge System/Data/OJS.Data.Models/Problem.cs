namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel;

    using OJS.Data.Contracts;

    public class Problem : DeletableEntity, IOrderable
    {
        private ICollection<Test> tests;
        private ICollection<ProblemResource> resources;
        private ICollection<Submission> submissions;

        public Problem()
        {
            this.tests = new HashSet<Test>();
            this.resources = new HashSet<ProblemResource>();
            this.submissions = new HashSet<Submission>();
        }

        public int Id { get; set; }

        public int OldId { get; set; }

        public int ContestId { get; set; }

        public virtual Contest Contest { get; set; }

        public string Name { get; set; }

        public short MaximumPoints { get; set; }

        /// <summary>
        /// Time limit for the task. Measured in milliseconds.
        /// </summary>
        public int TimeLimit { get; set; }

        /// <summary>
        /// Memory limit for the task. Measured in bytes.
        /// </summary>
        public long MemoryLimit { get; set; }

        /// <summary>
        /// Source code size limit (measured in bytes).
        /// </summary>
        public int? SourceCodeSizeLimit { get; set; }

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
    }
}
