namespace OJS.Web.Areas.Users.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using OJS.Common;
    using OJS.Data.Models;
    using OJS.Web.Areas.Users.Helpers;

    using Resource = Resources.Areas.Users.ViewModels.ProfileViewModels;

    public class UserProfileViewModel
    {
        public UserProfileViewModel(UserProfile profile)
        {
            this.Id = profile.Id;
            this.Username = profile.UserName;
            this.Email = profile.Email;
            this.FirstName = profile.UserSettings.FirstName;
            this.LastName = profile.UserSettings.LastName;
            this.City = profile.UserSettings.City;
            this.Age = profile.UserSettings.Age;
            this.Participations = new HashSet<UserParticipationViewModel>();
        }

        public string Id { get; set; }

        public string Username { get; set; }

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
            ErrorMessage = "Family_name_too_long",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(
            Name = "Family_name",
            ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string LastName { get; set; }

        [MaxLength(
            GlobalConstants.CityMaxLength,
            ErrorMessage = "City_too_long",
            ErrorMessageResourceType = typeof(Resource))]
        [Display(Name = "City", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public string City { get; set; }

        [Display(Name = "Age", ResourceType = typeof(Resource))]
        [NullDisplayFormat(ConvertEmptyStringToNull = true)]
        public byte? Age { get; set; }

        public IEnumerable<UserParticipationViewModel> Participations { get; set; }
    }
}