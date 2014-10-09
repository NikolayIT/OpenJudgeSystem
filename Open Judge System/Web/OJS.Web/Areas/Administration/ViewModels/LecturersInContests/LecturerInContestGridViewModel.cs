namespace OJS.Web.Areas.Administration.ViewModels.LecturersInContests
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;

    public class LecturerInContestGridViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<UserProfile, LecturerInContestGridViewModel>> ViewModel
        {
            get
            {
                return x =>
                    new LecturerInContestGridViewModel
                    {
                        UserId = x.Id,
                        UserName = x.UserName,
                        FirstName = x.UserSettings.FirstName ?? "Няма",
                        LastName = x.UserSettings.LastName ?? "Няма",
                        Email = x.Email
                    };
            }
        }

        public string UserId { get; set; }

        [Display(Name = "Потребителско име")]
        [Required(ErrorMessage = "Потребителското име е задължително.")]
        public string UserName { get; set; }

        [Display(Name = "Име")]
        public string FirstName { get; set; }

        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Display(Name = "E-mail")]
        public string Email { get; set; }
    }
}