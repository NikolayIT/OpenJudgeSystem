namespace OJS.Web.Areas.Users.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;

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
        [MaxLength(80)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [MaxLength(30, ErrorMessageResourceName = "First_name_too_long", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "First_name", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string FirstName { get; set; }

        [MaxLength(30, ErrorMessageResourceName = "Family_name_too_long", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Family_name", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string LastName { get; set; }

        [Display(Name = "Date_of_birth", ResourceType = typeof(Resource))]
        [NullDisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ConvertEmptyStringToNull = true)]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(30, ErrorMessageResourceName = "City_too_long", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "City", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string City { get; set; }

        [MaxLength(50, ErrorMessageResourceName = "Education_too_long", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Education_institution", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string EducationalInstitution { get; set; }

        [MaxLength(30, ErrorMessageResourceName = "Faculty_number_too_long", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Faculty_number", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string FacultyNumber { get; set; }

        [MaxLength(30, ErrorMessageResourceName = "Company_too_long", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Company", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string Company { get; set; }

        [MaxLength(30, ErrorMessageResourceName = "Job_title_too_long", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Job_title", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string JobTitle { get; set; }

        [Display(Name = "Age", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public byte? Age { get; set; }
    }
}