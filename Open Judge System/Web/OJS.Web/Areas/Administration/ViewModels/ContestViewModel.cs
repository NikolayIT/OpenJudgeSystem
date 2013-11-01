namespace OJS.Web.Areas.Administration.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;

    using OJS.Data.Models;

    public class ContestViewModel
    {
        public static Expression<Func<Contest, ContestViewModel>> FromContest
        {
            get
            {
                return contest => new ContestViewModel
                {
                    Id = contest.Id,
                    Name = contest.Name,
                    StartTime = contest.StartTime,
                    EndTime = contest.EndTime,
                    PracticeStartTime = contest.PracticeStartTime,
                    PracticeEndTime = contest.PracticeEndTime,
                    IsVisible = contest.IsVisible,
                    CreatedOn = contest.CreatedOn,
                    ModifiedOn = contest.ModifiedOn,
                    CategoryId = contest.CategoryId,
                    ContestPassword = contest.ContestPassword,
                    PracticePassword = contest.PracticePassword
                };
            }
        }

        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Display(Name = "Име")]
        [Required(ErrorMessage = "Заглавието е задължително!")]
        public string Name { get; set; }

        [Display(Name = "Начало")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "Край")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Начало упражнение")]
        public DateTime? PracticeStartTime { get; set; }

        [Display(Name = "Край упражнение")]
        public DateTime? PracticeEndTime { get; set; }

        [Display(Name = "Видимо")]
        public bool IsVisible { get; set; }

        [Display(Name = "Създадено на")]
        public DateTime? CreatedOn { get; set; }

        [Display(Name = "Модифицирано на")]
        public DateTime? ModifiedOn { get; set; }

        [Display(Name = "Категория")]
        public int? CategoryId { get; set; }

        [Display(Name = "Парола за състезание")]
        public string ContestPassword { get; set; }

        [Display(Name = "Парола за упражнение")]
        public string PracticePassword { get; set; }
    }
}