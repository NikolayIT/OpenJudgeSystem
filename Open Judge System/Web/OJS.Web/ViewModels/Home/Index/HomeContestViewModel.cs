namespace OJS.Web.ViewModels.Home.Index
{
    using System;
    using System.Linq.Expressions;
    using OJS.Data.Models;

    public class HomeContestViewModel
    {
        public static Expression<Func<Contest, HomeContestViewModel>> FromContest
        {
            get
            {
                return contest => new HomeContestViewModel
                {
                    Id = contest.Id,
                    Name = contest.Name,
                    EndTime = contest.EndTime
                };
            }
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime? EndTime { get; set; }
    }
}