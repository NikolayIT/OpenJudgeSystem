namespace OJS.Web.Areas.Administration.ViewModels.LecturersInContestCategories
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;

    public class LecturerInContestCategoryShortViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<LecturerInContestCategory, LecturerInContestCategoryShortViewModel>> ViewModel
        {
            get
            {
                return x =>
                    new LecturerInContestCategoryShortViewModel
                    {
                        ContestCategoryId = x.ContestCategoryId,
                        ContestCategoryName = x.ContestCategory.Name,
                        LecturerId = x.LecturerId
                    };
            }
        }

        [Display(Name = "№")]
        public int ContestCategoryId { get; set; }

        [Display(Name = "Категория")]
        public string ContestCategoryName { get; set; }

        public string LecturerId { get; set; }
    }
}