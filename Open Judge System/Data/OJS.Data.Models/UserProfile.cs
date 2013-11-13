namespace OJS.Data.Models
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Data.Contracts;
    using OJS.Data.Contracts.DataAnnotations;

    public class UserProfile : IdentityUser, IDeletableEntity, IAuditInfo
    {
        public UserProfile()
            : this(string.Empty, string.Empty)
        {
        }

        public UserProfile(string userName, string email)
            : base(userName)
        {
            this.UserName = userName;
            this.Email = email;
            this.UserSettings = new UserSettings();
            this.CreatedOn = DateTime.Now;
        }

        [Required]
        [Display(Name = "E-mail")]
        [MaxLength(80)]
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
        
        #region IDeletableEntity
        [Display(Name = "Изтрит?")]
        [Editable(false)]
        public bool IsDeleted { get; set; }

        [Display(Name = "Дата на изтриване")]
        [Editable(false)]
        [DataType(DataType.DateTime)]
        public DateTime? DeletedOn { get; set; }
        #endregion

        #region IAuditInfo
        [Display(Name = "Дата на създаване")]
        [Editable(false)]
        [DataType(DataType.DateTime)]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Specifies whether or not the CreatedOn property should be automatically set.
        /// </summary>
        [NotMapped]
        public bool PreserveCreatedOn { get; set; }

        [Display(Name = "Дата на промяна")]
        [Editable(false)]
        [DataType(DataType.DateTime)]
        public DateTime? ModifiedOn { get; set; }
        #endregion
    }
}
