namespace OJS.Web.Areas.Contests.ViewModels
{
    using System;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class ProblemListItemViewModel
    {
        private string name;

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

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value.Replace("#", "\\#");
            }
        }
    }
}