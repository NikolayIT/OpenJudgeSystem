namespace OJS.Web.Areas.Administration.ViewModels.Contest
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;

    using Resource = Resources.Areas.Administration.Contests.ViewModels.ShortContestAdministration;

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

        [Display(Name = "Name", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Name_required",
            ErrorMessageResourceType = typeof(Resource))]
        public string Name { get; set; }

        [Display(Name = "Start_time", ResourceType = typeof(Resource))]
        public DateTime? StartTime { get; set; }

        [Display(Name = "End_time", ResourceType = typeof(Resource))]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Category_name", ResourceType = typeof(Resource))]
        public string CategoryName { get; set; }
    }
}