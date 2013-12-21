namespace OJS.Web.ViewModels.Search
{
    using System;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class SearchResultViewModel
    {
        public static Expression<Func<Problem, SearchResultViewModel>> FromProblem
        {
            get
            {
                return problem => new SearchResultViewModel
                {
                    Id = problem.Id,
                    Name = problem.Name,
                    ParentId = problem.ContestId,
                    ParentName = problem.Contest.Name
                };
            }
        }

        public static Expression<Func<Contest, SearchResultViewModel>> FromContest
        {
            get
            {
                return contest => new SearchResultViewModel
                {
                    Id = contest.Id,
                    Name = contest.Name,
                    ParentId = contest.CategoryId,
                    ParentName = contest.Category.Name
                };
            }
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public int? ParentId { get; set; }

        public string ParentName { get; set; }
    }
}