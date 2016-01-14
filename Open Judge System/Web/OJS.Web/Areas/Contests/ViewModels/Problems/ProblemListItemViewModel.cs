namespace OJS.Web.Areas.Contests.ViewModels.Problems
{
    using System;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class ProblemListItemViewModel
    {
        public static Expression<Func<Problem, ProblemListItemViewModel>> FromProblem
        {
            get
            {
                return pr => new ProblemListItemViewModel
                {
                    ProblemId = pr.Id,
                    Name = pr.Name
                };
            }
        }

        public int ProblemId { get; set; }

        public string Name { get; set; }
    }
}