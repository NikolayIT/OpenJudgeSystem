namespace OJS.Web.Areas.Administration.ViewModels.Problem
{
    using System;
    using System.Linq.Expressions;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;

    public class ProblemViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<Problem, ProblemViewModel>> FromProblem
        {
            get
            {
                return problem => new ProblemViewModel
                {
                    Id = problem.Id,
                    Name = problem.Name,
                    ContestName = problem.ProblemGroup.Contest.Name
                };
            }
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string ContestName { get; set; }
    }
}