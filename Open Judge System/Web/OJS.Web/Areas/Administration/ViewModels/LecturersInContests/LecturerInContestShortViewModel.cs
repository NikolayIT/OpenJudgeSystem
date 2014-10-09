namespace OJS.Web.Areas.Administration.ViewModels.LecturersInContests
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;

    public class LecturerInContestShortViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<LecturerInContest, LecturerInContestShortViewModel>> ViewModel
        {
            get
            {
                return x =>
                    new LecturerInContestShortViewModel
                    {
                        ContestId = x.ContestId,
                        ContestName = x.Contest.Name,
                        LecturerId = x.LecturerId
                    };
            }
        }

        [Display(Name = "№")]
        public int ContestId { get; set; }

        [Display(Name = "Състезанието")]
        public string ContestName { get; set; }

        public string LecturerId { get; set; }
    }
}