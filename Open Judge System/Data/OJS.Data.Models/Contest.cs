namespace OJS.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Common;
    using OJS.Data.Contracts;

    public class Contest : DeletableEntity, IValidatableObject, IOrderable
    {
        private ICollection<ContestQuestion> questions;
        private ICollection<Problem> problems;
        private ICollection<Participant> participants;
        private ICollection<SubmissionType> submissionTypes;

        public Contest()
        {
            this.questions = new HashSet<ContestQuestion>();
            this.problems = new HashSet<Problem>();
            this.participants = new HashSet<Participant>();
            this.submissionTypes = new HashSet<SubmissionType>();
        }

        [Key]
        public int Id { get; set; }

        public int? OldId { get; set; }

        [MaxLength(GlobalConstants.ContestNameMaxLength)]
        [MinLength(GlobalConstants.ContestNameMinLength)]
        public string Name { get; set; }

        [Required]
        public bool IsVisible { get; set; }

        public int? CategoryId { get; set; }

        public virtual ContestCategory Category { get; set; }

        /// <remarks>
        /// If StartTime is null the contest cannot be competed.
        /// </remarks>
        public DateTime? StartTime { get; set; }

        /// <remarks>
        /// If EndTime is null the contest can be competed forever.
        /// </remarks>
        public DateTime? EndTime { get; set; }

        /// <remarks>
        /// If ContestPassword is null the contest can be competed by everyone without require a password.
        /// If the ContestPassword is not null the contest participant should provide a valid password.
        /// </remarks>
        [MaxLength(GlobalConstants.ContestPasswordMaxLength)]
        public string ContestPassword { get; set; }

        /// <remarks>
        /// If PracticePassword is null the contest can be practiced by everyone without require a password.
        /// If the PracticePassword is not null the practice participant should provide a valid password.
        /// </remarks>
        [MaxLength(GlobalConstants.ContestPasswordMaxLength)]
        public string PracticePassword { get; set; }

        /// <remarks>
        /// If PracticeStartTime is null the contest cannot be practiced.
        /// </remarks>
        public DateTime? PracticeStartTime { get; set; }

        /// <remarks>
        /// If PracticeEndTime is null the contest can be practiced forever.
        /// </remarks>
        public DateTime? PracticeEndTime { get; set; }

        [DefaultValue(0)]
        public int LimitBetweenSubmissions { get; set; }

        [DefaultValue(0)]
        public int OrderBy { get; set; }

        public string Description { get; set; }

        public virtual ICollection<ContestQuestion> Questions
        {
            get { return this.questions; }
            set { this.questions = value; }
        }

        public virtual ICollection<Problem> Problems
        {
            get { return this.problems; }
            set { this.problems = value; }
        }

        public virtual ICollection<Participant> Participants
        {
            get { return this.participants; }
            set { this.participants = value; }
        }

        public virtual ICollection<SubmissionType> SubmissionTypes
        {
            get { return this.submissionTypes; }
            set { this.submissionTypes = value; }
        }

        [NotMapped]
        public bool CanBeCompeted
        {
            get
            {
                if (!this.IsVisible)
                {
                    return false;
                }

                if (this.IsDeleted)
                {
                    return false;
                }

                if (!this.StartTime.HasValue)
                {
                    // Cannot be competed
                    return false;
                }

                if (!this.EndTime.HasValue)
                {
                    // Compete forever
                    return this.StartTime <= DateTime.Now;
                }

                return this.StartTime <= DateTime.Now && DateTime.Now <= this.EndTime;
            }
        }

        [NotMapped]
        public bool CanBePracticed
        {
            get
            {
                if (!this.IsVisible)
                {
                    return false;
                }

                if (this.IsDeleted)
                {
                    return false;
                }

                if (!this.PracticeStartTime.HasValue)
                {
                    // Cannot be practiced
                    return false;
                }

                if (!this.PracticeEndTime.HasValue)
                {
                    // Practice forever
                    return this.PracticeStartTime <= DateTime.Now;
                }

                return this.PracticeStartTime <= DateTime.Now && DateTime.Now <= this.PracticeEndTime;
            }
        }

        [NotMapped]
        public bool ResultsArePubliclyVisible
        {
            get
            {
                if (!this.IsVisible)
                {
                    return false;
                }

                if (this.IsDeleted)
                {
                    return false;
                }

                if (!this.StartTime.HasValue)
                {
                    // Cannot be competed
                    return false;
                }

                return this.EndTime.HasValue && this.EndTime.Value <= DateTime.Now;
            }
        }

        [NotMapped]
        public bool HasContestPassword
        {
            get
            {
                return this.ContestPassword != null;
            }
        }

        [NotMapped]
        public bool HasPracticePassword
        {
            get
            {
                return this.PracticePassword != null;
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            var contest = validationContext.ObjectInstance as Contest;
            if (contest == null)
            {
                return validationResults;
            }

            if (contest.StartTime.HasValue && contest.EndTime.HasValue && contest.StartTime.Value > contest.EndTime.Value)
            {
                validationResults.Add(
                    new ValidationResult("StartTime can not be after EndTime", new[] { "StartTime", "EndTime" }));
            }

            return validationResults;
        }

        public override string ToString()
        {
            return string.Format("#{0} {1}", this.Id, this.Name);
        }
    }
}
