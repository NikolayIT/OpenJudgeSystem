namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Common.Extensions;
    using OJS.Common.Models;

    public class SubmissionType
    {
        private ICollection<Contest> contests;

        public SubmissionType()
        {
            this.contests = new HashSet<Contest>();
        }

        [Key]
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
        
        [NotMapped]
        public string FileNameExtension
        {
            get
            {
                string extension = (this.ExecutionStrategyType.GetFileExtension()
                                    ?? this.CompilerType.GetFileExtension()) ?? string.Empty;

                return extension;
            }
        }
    }
}
