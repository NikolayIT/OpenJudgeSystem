namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel;

    using OJS.Common.Models;

    public class SubmissionType
    {
        private ICollection<Contest> contests;

        public SubmissionType()
        {
            this.contests = new HashSet<Contest>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        [DefaultValue(false)]
        public bool IsSelectedByDefault { get; set; }

        public ExecutionStrategyType ExecutionStrategyType { get; set; }

        public CompilerType CompilerType { get; set; }

        public string AdditionalCompilerArguments { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Contest> Contests
        {
            get { return this.contests; }
            set { this.contests = value; }
        }
    }
}
