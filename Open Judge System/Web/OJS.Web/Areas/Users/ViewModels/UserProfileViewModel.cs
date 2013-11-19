namespace OJS.Web.Areas.Users.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;

    using OJS.Data.Models;
    using System.ComponentModel.DataAnnotations;

    public class UserProfileViewModel
    {
        public UserProfileViewModel(UserProfile profile)
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

        [Display(Name = "E-mail")]
        [MaxLength(80)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [MaxLength(30, ErrorMessage = "Въведеното име е твърде дълго")]
        [Display(Name = "Име")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public string FirstName { get; set; }

        [MaxLength(30, ErrorMessage = "Въведената фамилия е твърде дълга")]
        [Display(Name = "Фамилия")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public string LastName { get; set; }

        [Display(Name = "Дата на раждане")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(30, ErrorMessage = "Въведеният град е твърде дълъг")]
        [Display(Name = "Град")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public string City { get; set; }

        [MaxLength(50, ErrorMessage = "Въведеното образование е твърде дълго")]
        [Display(Name = "Образование")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public string EducationalInstitution { get; set; }

        [MaxLength(30, ErrorMessage = "Въведеният факултетен номер е твърде дълъг")]
        [Display(Name = "Факултетен номер")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public string FacultyNumber { get; set; }

        [MaxLength(30, ErrorMessage = "Въведената месторабота е твърде дълга")]
        [Display(Name = "Месторабота")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public string Company { get; set; }

        [MaxLength(30, ErrorMessage = "Въведената позиция е твърде дълга")]
        [Display(Name = "Позиция")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public string JobTitle { get; set; }

        [Display(Name = "Възраст")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public byte? Age { get; set; }
    }
}