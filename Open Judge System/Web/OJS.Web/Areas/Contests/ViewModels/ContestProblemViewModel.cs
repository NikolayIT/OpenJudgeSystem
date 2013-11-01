namespace OJS.Web.Areas.Contests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;
    using OJS.Data.Models;

    public class ContestProblemViewModel
    {
        public ContestProblemViewModel(Problem problem)
        {
            this.ProblemId = problem.Id;
            this.Name = problem.Name;
            this.ContestId = problem.ContestId;
            this.Resources = problem.Resources.AsQueryable().Select(ContestProblemResourceViewModel.FromResource);
        }

        public ContestProblemViewModel()
        {
            this.Resources = new HashSet<ContestProblemResourceViewModel>();
        }

        public static Expression<Func<Problem, ContestProblemViewModel>> FromProblem
        {
            get
            {
                return problem => new ContestProblemViewModel
                {
                    Name = problem.Name,
                    ProblemId = problem.Id,
                    ContestId = problem.ContestId,
                    Resources = problem.Resources.AsQueryable().Select(ContestProblemResourceViewModel.FromResource)
                };
            }
        }

        public int ContestId { get; set; }

        public int ProblemId { get; set; }

        public string Name { get; set; }

        public IEnumerable<ContestProblemResourceViewModel> Resources { get; set; }
    }
}