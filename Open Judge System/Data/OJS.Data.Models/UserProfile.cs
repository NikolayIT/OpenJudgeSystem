namespace OJS.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Common;
    using OJS.Data.Contracts;
    using OJS.Data.Contracts.DataAnnotations;

    public class UserProfile : IdentityUser, IDeletableEntity, IAuditInfo
    {
        public UserProfile()
            : this(string.Empty, string.Empty)
        {
        }

        public UserProfile(string userName, string email)
            : base(userName) => this.Email = email;

        [Required]
        [MaxLength(GlobalConstants.EmailMaxLength)]
        [MinLength(GlobalConstants.EmailMinLength)]
        [RegularExpression(GlobalConstants.EmailRegEx)]
        [IsUnicode(false)]
        [DataType(DataType.EmailAddress)]
        public override string Email { get; set; }

        public UserSettings UserSettings { get; set; } = new UserSettings();

        public Guid? ForgottenPasswordToken { get; set; }

        public bool IsDeleted { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DeletedOn { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        /// <summary>
        /// Specifies whether or not the CreatedOn property should be automatically set.
        /// </summary>
        [NotMapped]
        public bool PreserveCreatedOn { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ModifiedOn { get; set; }

        public virtual ICollection<LecturerInContest> LecturerInContests { get; set; } =
            new HashSet<LecturerInContest>();

        public virtual ICollection<LecturerInContestCategory> LecturerInContestCategories { get; set; } =
            new HashSet<LecturerInContestCategory>();

        public virtual ICollection<ExamGroup> ExamGroups { get; set; } = new HashSet<ExamGroup>();
    }
}