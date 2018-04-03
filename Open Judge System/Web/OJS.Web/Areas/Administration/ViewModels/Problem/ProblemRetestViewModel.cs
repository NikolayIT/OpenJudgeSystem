namespace OJS.Web.Areas.Administration.ViewModels.Problem
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;

    public class ProblemRetestViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<Problem, ProblemRetestViewModel>> FromProblem =>
            problem => new ProblemRetestViewModel
            {
                Id = problem.Id,
                Name = problem.Name,
                ContestName = problem.ProblemGroup.Contest.Name,
                ContestId = problem.ProblemGroup.ContestId,
                SubmissionsCount = problem.Submissions.Count(s => !s.IsDeleted)
            };

        public int Id { get; set; }

        public string Name { get; set; }

        public string ContestName { get; set; }

        public int ContestId { get; set; }

        public int SubmissionsCount { get; set; }
    }
}