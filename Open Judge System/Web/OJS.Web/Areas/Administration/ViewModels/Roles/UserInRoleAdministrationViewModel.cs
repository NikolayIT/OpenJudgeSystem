namespace OJS.Web.Areas.Administration.ViewModels.Roles
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;

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
                    FirstName = us.UserSettings.FirstName ?? "Няма",
                    LastName = us.UserSettings.LastName ?? "Няма",
                    Email = us.Email
                };
            }
        }

        public string UserId { get; set; }

        [Display(Name = "Потребителско име")]
        [Required(ErrorMessage = "Потребителското име е задължително!")]
        public string UserName { get; set; }

        [Display(Name = "Име")]
        public string FirstName { get; set; }

        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Display(Name = "E-mail")]
        public string Email { get; set; }
    }
}