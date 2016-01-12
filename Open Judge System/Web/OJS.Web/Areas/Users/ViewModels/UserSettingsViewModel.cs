namespace OJS.Web.Areas.Users.ViewModels
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using OJS.Common;
    using OJS.Data.Models;
    using OJS.Web.Areas.Users.Helpers;

    using Resource = Resources.Areas.Users.ViewModels.ProfileViewModels;

    public class UserSettingsViewModel
    {
        public UserSettingsViewModel()
        {
        }

        public UserSettingsViewModel(UserProfile profile)
        {
            this.Username = profile.UserName;
            this.Email = profile.Email;
            this.FirstName = profile.UserSettings.FirstName;
            this.LastName = profile.UserSettings.LastName;
            this.DateOfBirth = profile.UserSettings.DateOfBirth;
            this.City = profile.UserSettings.City;
            this.EducationalInstitution = profile.UserSettings.EducationalInstitution;
            this.FacultyNumber = profile.UserSettings.FacultyNumber;
            this.Company = profile.UserSettings.Company;
            this.JobTitle = profile.UserSettings.JobTitle;
            this.Age = profile.UserSettings.Age;
        }

        public string Username { get; set; }

        [Display(Name = "Email", ResourceType = typeof(Resource))]
        [MaxLength(GlobalConstants.EmailMaxLength)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [MaxLength(
            GlobalConstants.NameMaxLength,
            ErrorMessageResourceName = "First_name_too_long",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "First_name", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string FirstName { get; set; }

        [MaxLength(
            GlobalConstants.NameMaxLength,
            ErrorMessageResourceName = "Family_name_too_long",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Family_name", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string LastName { get; set; }

        [Display(Name = "Date_of_birth", ResourceType = typeof(Resource))]
        [NullDisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ConvertEmptyStringToNull = true)]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(
            GlobalConstants.CityMaxLength,
            ErrorMessageResourceName = "City_too_long",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "City", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string City { get; set; }

        [Display(Name = "Education_institution", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string EducationalInstitution { get; set; }

        [MaxLength(
            GlobalConstants.FacultyNumberMaxLength,
            ErrorMessageResourceName = "Faculty_number_too_long",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Faculty_number", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string FacultyNumber { get; set; }

        [MaxLength(
            GlobalConstants.CompanyMaxLength,
            ErrorMessageResourceName = "Company_too_long",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Company", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string Company { get; set; }

        [MaxLength(
            GlobalConstants.JobTitleMaxLength,
            ErrorMessageResourceName = "Job_title_too_long",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Job_title", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string JobTitle { get; set; }

        [Display(Name = "Age", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        [Range(0, byte.MaxValue)]
        public byte? Age { get; set; }
    }
}