namespace OJS.Web.Areas.Administration.ViewModels.User
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common.Attributes;
    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;

    using static OJS.Common.Constants.EditorTemplateConstants;

    using Resource = Resources.Areas.Administration.Users.ViewModels.UserProfileAdministration;

    public class UserProfileSimpleAdministrationViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<UserProfile, UserProfileSimpleAdministrationViewModel>> FromUserProfile =>
            user => new UserProfileSimpleAdministrationViewModel
            {
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.UserSettings.FirstName ?? Resource.Missing,
                LastName = user.UserSettings.LastName ?? Resource.Missing
            };

        [Display(Name = "№")]
        [HiddenInput(DisplayValue = false)]
        public string UserId { get; set; }

        [Display(Name = nameof(Resource.UserName), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Username_required),
            ErrorMessageResourceType = typeof(Resource))]
        public string Username { get; set; }

        [DataType(DataType.EmailAddress)]
        [UIHint(SingleLineText)]
        public string Email { get; set; }

        [Display(Name = nameof(Resource.First_name), ResourceType = typeof(Resource))]
        [LocalizedDisplayFormat(
            NullDisplayTextResourceName = nameof(Resource.Null_display_text),
            NullDisplayTextResourceType = typeof(Resource),
            ConvertEmptyStringToNull = true)]
        [UIHint(SingleLineText)]
        public string FirstName { get; set; }

        [Display(Name = nameof(Resource.Last_name), ResourceType = typeof(Resource))]
        [LocalizedDisplayFormat(
            NullDisplayTextResourceName = nameof(Resource.Null_display_text),
            NullDisplayTextResourceType = typeof(Resource),
            ConvertEmptyStringToNull = true)]
        [UIHint(SingleLineText)]
        public string LastName { get; set; }
    }
}