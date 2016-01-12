namespace OJS.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Common;
    using OJS.Data.Contracts;
    using OJS.Data.Contracts.DataAnnotations;

    public class UserProfile : IdentityUser, IDeletableEntity, IAuditInfo
    {
        private ICollection<LecturerInContest> lecturerInContests; 

        public UserProfile()
            : this(string.Empty, string.Empty)
        {
        }

        public UserProfile(string userName, string email)
            : base(userName)
        {
            this.Email = email;
            this.UserSettings = new UserSettings();
            this.lecturerInContests = new HashSet<LecturerInContest>();
            this.CreatedOn = DateTime.Now;
        }

        [Required]
        [MaxLength(GlobalConstants.EmailMaxLength)]
        [MinLength(GlobalConstants.EmailMinLength)]
        [RegularExpression(GlobalConstants.EmailRegEx)]
        [IsUnicode(false)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        /// <summary>
        /// This property is true when the user comes from the old system and is not preregistered in the new system.
        /// </summary>
        [DefaultValue(false)]
        public bool IsGhostUser { get; set; }

        public int? OldId { get; set; }

        public UserSettings UserSettings { get; set; }

        public Guid? ForgottenPasswordToken { get; set; }

        public virtual ICollection<LecturerInContest> LecturerInContests
        {
            get { return this.lecturerInContests; }
            set { this.lecturerInContests = value; }
        }

        public bool IsDeleted { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DeletedOn { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Specifies whether or not the CreatedOn property should be automatically set.
        /// </summary>
        [NotMapped]
        public bool PreserveCreatedOn { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ModifiedOn { get; set; }
    }
}
