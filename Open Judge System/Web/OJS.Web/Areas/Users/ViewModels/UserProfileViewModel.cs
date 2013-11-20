namespace OJS.Web.Areas.Users.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;

    using OJS.Data.Models;

    public class UserProfileViewModel
    {
        public UserProfileViewModel(UserProfile profile)
        {
            this.Username = profile.UserName;
            this.FirstName = profile.UserSettings.FirstName;
            this.LastName = profile.UserSettings.LastName;
            this.City = profile.UserSettings.City;
            this.Age = profile.UserSettings.Age;
            this.Participations = new HashSet<UserParticipationViewModel>();
        }

        public string Username { get; set; }

        [MaxLength(30, ErrorMessage = "Въведеното име е твърде дълго")]
        [Display(Name = "Име")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public string FirstName { get; set; }

        [MaxLength(30, ErrorMessage = "Въведената фамилия е твърде дълга")]
        [Display(Name = "Фамилия")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public string LastName { get; set; }

        [MaxLength(30, ErrorMessage = "Въведеният град е твърде дълъг")]
        [Display(Name = "Град")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public string City { get; set; }

        [Display(Name = "Възраст")]
        [DisplayFormat(NullDisplayText = "Няма информация", ConvertEmptyStringToNull = true)]
        public byte? Age { get; set; }

        public IEnumerable<UserParticipationViewModel> Participations { get; set; }
    }
}