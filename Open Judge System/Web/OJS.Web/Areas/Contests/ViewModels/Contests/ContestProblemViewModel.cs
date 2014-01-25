namespace OJS.Web.Areas.Contests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class ContestProblemViewModel
    {
        private int memoryLimitInBytes;

        private int timeLimitInMs;

        public ContestProblemViewModel(Problem problem)
        {
            this.ProblemId = problem.Id;
            this.Name = problem.Name;
            this.ContestId = problem.ContestId;
            this.ShowResults = problem.ShowResults;
            this.Resources = problem.Resources.AsQueryable()
                                                .OrderBy(x => x.OrderBy)
                                                .Where(x => !x.IsDeleted)
                                                .Select(ContestProblemResourceViewModel.FromResource);
            this.TimeLimit = problem.TimeLimit;
            this.MemoryLimit = problem.MemoryLimit;
            this.CheckerName = problem.Checker.Name;
            this.CheckerDescription = problem.Checker.Description;
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
                    MemoryLimit = problem.MemoryLimit,
                    TimeLimit = problem.TimeLimit,
                    ShowResults = problem.ShowResults,
                    CheckerName = problem.Checker.Name,
                    CheckerDescription = problem.Checker.Description,
                    Resources = problem.Resources.AsQueryable()
                                                            .Where(x => !x.IsDeleted)
                                                            .OrderBy(x => x.OrderBy)
                                                            .Select(ContestProblemResourceViewModel.FromResource)
                };
            }
        }

        public int ContestId { get; set; }

        public int ProblemId { get; set; }

        public string Name { get; set; }

        public bool ShowResults { get; set; }

        public double MemoryLimit
        {
            get
            {
                return (double)this.memoryLimitInBytes / 1024 / 1024;
            }

            set
            {
                this.memoryLimitInBytes = (int)value;
            }
        }

        public double TimeLimit
        {
            get
            {
                return this.timeLimitInMs / 1000.00;
            }

            set
            {
                this.timeLimitInMs = (int)value;
            }
        }

        public string CheckerName { get; set; }

        public string CheckerDescription { get; set; }

        public IEnumerable<ContestProblemResourceViewModel> Resources { get; set; }
    }
}