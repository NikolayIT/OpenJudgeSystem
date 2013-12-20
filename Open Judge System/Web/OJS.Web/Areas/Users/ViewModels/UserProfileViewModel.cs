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

        [MaxLength(30, ErrorMessageResourceName = "First_name_too_long", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "First_name", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string FirstName { get; set; }

        [MaxLength(30, ErrorMessage = "Family_name_too_long", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "Family_name", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string LastName { get; set; }

        [MaxLength(30, ErrorMessage = "City_too_long", ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "City", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string City { get; set; }

        [Display(Name = "Age", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public byte? Age { get; set; }

        public IEnumerable<UserParticipationViewModel> Participations { get; set; }
    }
}