namespace OJS.Web.Areas.Contests.ViewModels.Contests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Common.Models;
    using OJS.Data.Models;

    public class ContestProblemViewModel
    {
        private int memoryLimitInBytes;

        private int timeLimitInMs;

        private int? fileSizeLimitInBytes;

        public ContestProblemViewModel() => this.Resources = new HashSet<ContestProblemResourceViewModel>();

        public static Expression<Func<Problem, ContestProblemViewModel>> FromProblem
        {
            get
            {
                return problem => new ContestProblemViewModel
                {
                    Name = problem.Name,
                    OrderBy = problem.OrderBy,
                    ProblemId = problem.Id,
                    ContestId = problem.ProblemGroup.ContestId,
                    MemoryLimit = problem.MemoryLimit,
                    TimeLimit = problem.TimeLimit,
                    FileSizeLimit = problem.SourceCodeSizeLimit,
                    ShowResults = problem.ShowResults,
                    IsExcludedFromHomework = problem.ProblemGroup.Type == ProblemGroupType.ExcludedFromHomework,
                    CheckerName = problem.Checker.Name,
                    CheckerDescription = problem.Checker.Description,
                    MaximumPoints = problem.MaximumPoints,
                    Resources = problem.Resources
                        .AsQueryable()
                        .Where(r => !r.IsDeleted)
                        .OrderBy(r => r.OrderBy)
                        .Select(ContestProblemResourceViewModel.FromResource)
                };
            }
        }

        public int ContestId { get; set; }

        public int ProblemId { get; set; }

        public string Name { get; set; }

        public int OrderBy { get; set; }

        public int MaximumPoints { get; set; }

        public bool ShowResults { get; set; }

        public bool IsExcludedFromHomework { get; set; }

        public double MemoryLimit
        {
            get => (double)this.memoryLimitInBytes / 1024 / 1024;

            set => this.memoryLimitInBytes = (int)value;
        }

        public double TimeLimit
        {
            get => this.timeLimitInMs / 1000.00;

            set => this.timeLimitInMs = (int)value;
        }

        public double? FileSizeLimit
        {
            get
            {
                if (!this.fileSizeLimitInBytes.HasValue)
                {
                    return null;
                }

                return (double)this.fileSizeLimitInBytes / 1024;
            }

            set => this.fileSizeLimitInBytes = (int?)value;
        }

        public string CheckerName { get; set; }

        public string CheckerDescription { get; set; }

        public IEnumerable<ContestProblemResourceViewModel> Resources { get; set; }

        public bool UserHasAdminRights { get; set; }
    }
}
