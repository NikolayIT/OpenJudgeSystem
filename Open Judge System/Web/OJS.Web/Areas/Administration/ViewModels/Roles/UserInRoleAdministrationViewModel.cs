namespace OJS.Web.Areas.Administration.ViewModels.Roles
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;

    using Resource = Resources.Areas.Administration.Roles.ViewModels.RolesViewModels;

    public class UserInRoleAdministrationViewModel
    {
        [ExcludeFromExcel]    
        public static Expression<Func<UserProfile, UserInRoleAdministrationViewModel>> ViewModel
        {
            get
            {
                return us => new UserInRoleAdministrationViewModel
                {
                    UserId = us.Id,
                    UserName = us.UserName,
                    FirstName = us.UserSettings.FirstName ?? Resource.Not_entered,
                    LastName = us.UserSettings.LastName ?? Resource.Not_entered,
                    Email = us.Email
                };
            }
        }

        public string UserId { get; set; }

        [Display(Name = "UserName", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "UserName_required",
            ErrorMessageResourceType = typeof(Resource))]
        public string UserName { get; set; }

        [Display(Name = "Name", ResourceType = typeof(Resource))]
        public string FirstName { get; set; }

        [Display(Name = "Last_name", ResourceType = typeof(Resource))]
        public string LastName { get; set; }

        [Display(Name = "Email", ResourceType = typeof(Resource))]
        public string Email { get; set; }
    }
}