namespace OJS.Web.Areas.Administration.ViewModels.Contest
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;

    public class ShortContestAdministrationViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<Contest, ShortContestAdministrationViewModel>> FromContest
        {
            get
            {
                return contest => new ShortContestAdministrationViewModel
                {
                    Id = contest.Id,
                    Name = contest.Name,
                    StartTime = contest.StartTime,
                    EndTime = contest.EndTime,
                    CategoryName = contest.Category.Name,
                };
            }
        }

        public int Id { get; set; }

        [Display(Name = "Име")]
        [Required(ErrorMessage = "Заглавието е задължително!")]
        public string Name { get; set; }

        [Display(Name = "Начало")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "Край")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Категория")]
        public string CategoryName { get; set; }
    }
}