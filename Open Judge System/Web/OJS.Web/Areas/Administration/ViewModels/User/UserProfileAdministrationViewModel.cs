namespace OJS.Web.Areas.Administration.ViewModels.User
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Common.Attributes;
    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    using Resource = Resources.Areas.Administration.Users.ViewModels.UserProfileAdministration;

    public class UserProfileAdministrationViewModel : AdministrationViewModel<UserProfile>
    {
        [ExcludeFromExcel]
        public static Expression<Func<UserProfile, UserProfileAdministrationViewModel>> ViewModel
        {
            get
            {
                return user => new UserProfileAdministrationViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.UserSettings.FirstName,
                    LastName = user.UserSettings.LastName,
                    City = user.UserSettings.City,
                    EducationalInstitution = user.UserSettings.EducationalInstitution,
                    FacultyNumber = user.UserSettings.FacultyNumber,
                    DateOfBirth = user.UserSettings.DateOfBirth,
                    Company = user.UserSettings.Company,
                    JobTitle = user.UserSettings.JobTitle,
                    CreatedOn = user.CreatedOn,
                    ModifiedOn = user.ModifiedOn,
                };
            }
        }

        [Display(Name = "№")]
        [HiddenInput(DisplayValue = false)]
        public string Id { get; set; }

        [Display(Name = "UserName", ResourceType = typeof(Resource))]
        [UIHint("NonEditable")]
        public string UserName { get; set; }

        [DataType(DataType.EmailAddress)]
        [RegularExpression(
            GlobalConstants.EmailRegEx,
            ErrorMessageResourceName = "Mail_invalid",
            ErrorMessageResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Mail_required",
            ErrorMessageResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.EmailMaxLength,
            ErrorMessageResourceName = "Mail_length",
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint("SingleLineText")]
        public string Email { get; set; }

        [Display(Name = "First_name", ResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.NameMaxLength,
            ErrorMessageResourceName = "First_name_length",
            ErrorMessageResourceType = typeof(Resource))]
        [LocalizedDisplayFormat(
            NullDisplayTextResourceName = "Null_display_text",
            NullDisplayTextResourceType = typeof(Resource),
            ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string FirstName { get; set; }

        [Display(Name = "Last_name", ResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.NameMaxLength,
            ErrorMessageResourceName = "Last_name_length",
            ErrorMessageResourceType = typeof(Resource))]
        [LocalizedDisplayFormat(
            NullDisplayTextResourceName = "Null_display_text",
            NullDisplayTextResourceType = typeof(Resource),
            ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string LastName { get; set; }

        [Display(Name = "City", ResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.CityMaxLength,
            ErrorMessageResourceName = "City_length",
            ErrorMessageResourceType = typeof(Resource))]
        [LocalizedDisplayFormat(
            NullDisplayTextResourceName = "Null_display_text",
            NullDisplayTextResourceType = typeof(Resource),
            ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string City { get; set; }

        [Display(Name = "Educational_institution", ResourceType = typeof(Resource))]
        ////[StringLength(
        ////    GlobalConstants.EducationalInstitutionMaxLength,
        ////    ErrorMessageResourceName = "Educational_institution_length",
        ////    ErrorMessageResourceType = typeof(Resource))]
        [LocalizedDisplayFormat(
            NullDisplayTextResourceName = "Null_display_text",
            NullDisplayTextResourceType = typeof(Resource),
            ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string EducationalInstitution { get; set; }

        [Display(Name = "Faculty_number", ResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.FacultyNumberMaxLength,
            ErrorMessageResourceName = "Faculty_number_length",
            ErrorMessageResourceType = typeof(Resource))]
        [LocalizedDisplayFormat(
            NullDisplayTextResourceName = "Null_display_text",
            NullDisplayTextResourceType = typeof(Resource),
            ConvertEmptyStringToNull = true)]
        [UIHint("PositiveInteger")]
        public string FacultyNumber { get; set; }

        [Display(Name = "Date_of_birth", ResourceType = typeof(Resource))]
        [LocalizedDisplayFormat(
            NullDisplayTextResourceName = "Null_display_text",
            NullDisplayTextResourceType = typeof(Resource),
            ConvertEmptyStringToNull = true,
            DataFormatString = "{0:dd-MM-yyyy}")]
        [DataType(DataType.Date)]
        [UIHint("Date")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Company", ResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.CompanyMaxLength,
            ErrorMessageResourceName = "Company_length",
            ErrorMessageResourceType = typeof(Resource))]
        [LocalizedDisplayFormat(
            NullDisplayTextResourceName = "Null_display_text",
            NullDisplayTextResourceType = typeof(Resource),
            ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string Company { get; set; }

        [Display(Name = "Job_title", ResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.JobTitleMaxLength,
            ErrorMessageResourceName = "Job_title_length",
            ErrorMessageResourceType = typeof(Resource))]
        [LocalizedDisplayFormat(
            NullDisplayTextResourceName = "Null_display_text",
            NullDisplayTextResourceType = typeof(Resource),
            ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string JobTitle { get; set; }

        [Display(Name = "Age", ResourceType = typeof(Resource))]
        [LocalizedDisplayFormat(
            NullDisplayTextResourceName = "Null_display_text",
            NullDisplayTextResourceType = typeof(Resource),
            ConvertEmptyStringToNull = true)]
        [UIHint("NonEditable")]
        public byte Age => Calculator.Age(this.DateOfBirth) ?? default(byte);

        public override UserProfile GetEntityModel(UserProfile model = null)
        {
            model = model ?? new UserProfile();

            model.Id = this.Id;
            model.UserName = this.UserName;
            model.Email = this.Email;
            model.UserSettings = new UserSettings
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                City = this.City,
                EducationalInstitution = this.EducationalInstitution,
                FacultyNumber = this.FacultyNumber,
                DateOfBirth = this.DateOfBirth,
                Company = this.Company,
                JobTitle = this.JobTitle,
            };
            model.CreatedOn = this.CreatedOn.GetValueOrDefault();
            model.ModifiedOn = this.ModifiedOn;

            return model;
        }
    }
}