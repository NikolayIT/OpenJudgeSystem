namespace OJS.Web.ViewModels.Home.Index
{
    using System;
    using System.Linq.Expressions;
    using OJS.Data.Models;

    public class IndexContestViewModel
    {
        public IndexContestViewModel()
        {

        }

        public static Expression<Func<Contest, IndexContestViewModel>> FromContest
        {
            get
            {
                return contest => new IndexContestViewModel
                {
                    Id = contest.Id,
                    Name = contest.Name,
                    StartTime = contest.StartTime,
                    EndTime = contest.EndTime,
                };
            }
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}