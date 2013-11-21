namespace OJS.Web.Areas.Administration.ViewModels.User
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    public class UserProfileAdministrationViewModel : AdministrationViewModel
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
                    IsGhostUser = user.IsGhostUser,
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

        [ExcludeFromExcel]
        public UserProfile ToEntity
        {
            get
            {
                return new UserProfile
                {
                    Id = this.Id,
                    UserName = this.UserName,
                    Email = this.Email,
                    UserSettings = new UserSettings
                    {
                        FirstName = this.FirstName,
                        LastName = this.LastName,
                        City = this.City,
                        EducationalInstitution = this.EducationalInstitution,
                        FacultyNumber = this.FacultyNumber,
                        DateOfBirth = this.DateOfBirth,
                        Company = this.Company,
                        JobTitle = this.JobTitle,
                    },
                    CreatedOn = this.CreatedOn.GetValueOrDefault(),
                    ModifiedOn = this.ModifiedOn,
                };
            }
        }

        [Display(Name = "№")]
        [HiddenInput(DisplayValue = false)]
        public string Id { get; set; }

        [Display(Name = "Потребителско име")]
        [UIHint("NonEditable")]
        public string UserName { get; set; }

        [Display(Name = "Име")]
        [StringLength(30, ErrorMessage = "Въведеният e-mail е твърде дълъг")]
        [DataType(DataType.EmailAddress)]
        [UIHint("SingleLineText")]
        public string Email { get; set; }

        [Display(Name = "От старата система?")]
        [HiddenInput(DisplayValue = false)]
        public bool IsGhostUser { get; set; }

        [Display(Name = "Име")]
        [StringLength(30, ErrorMessage = "Въведеното име е твърде дълго")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string FirstName { get; set; }

        [Display(Name = "Фамилия")]
        [StringLength(30, ErrorMessage = "Въведената фамилия е твърде дълга")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string LastName { get; set; }

        [Display(Name = "Град")]
        [StringLength(30, ErrorMessage = "Въведеният град е твърде дълъг")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string City { get; set; }

        [Display(Name = "Образование")]
        [StringLength(50, ErrorMessage = "Въведеното образование е твърде дълго")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string EducationalInstitution { get; set; }

        [Display(Name = "Факултетен номер")]
        [StringLength(30, ErrorMessage = "Въведеният факултетен номер е твърде дълъг")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [UIHint("PositiveInteger")]
        public string FacultyNumber { get; set; }

        [Display(Name = "Дата на раждане")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [DataType(DataType.Date)]
        [UIHint("Date")]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Месторабота")]
        [StringLength(30, ErrorMessage = "Въведената месторабота е твърде дълга")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string Company { get; set; }

        [Display(Name = "Позиция")]
        [StringLength(30, ErrorMessage = "Въведената позиция е твърде дълга")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [UIHint("SingleLineText")]
        public string JobTitle { get; set; }

        [Display(Name = "Възраст")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        [UIHint("NonEditable")]
        public byte Age
        {
            get
            {
                return Calculator.Age(this.DateOfBirth) ?? default(byte);
            }
        }
    }
}