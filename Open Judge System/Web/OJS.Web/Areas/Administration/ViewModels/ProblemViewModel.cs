namespace OJS.Web.Areas.Administration.ViewModels
{
    using Antlr.Runtime.Misc;
    using System.Linq.Expressions;
    using OJS.Data.Models;

    public class ProblemViewModel
    {
        public static Expression<Func<Problem, ProblemViewModel>> FromProblem
        {
            get
            {
                return problem => new ProblemViewModel
                {
                    Id = problem.Id,
                    Name = problem.Name,
                    ContestName = problem.Contest.Name
                };
            }
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string ContestName { get; set; }
    }
}