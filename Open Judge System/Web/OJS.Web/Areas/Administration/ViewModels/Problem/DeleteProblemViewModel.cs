namespace OJS.Web.Areas.Administration.ViewModels.Problem
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common.Extensions;
    using OJS.Data.Models;

    using Resource = Resources.Areas.Administration.Problems.ViewModels.DetailedProblem;

    public class DeleteProblemViewModel
    {
        public static Expression<Func<Problem, DeleteProblemViewModel>> FromProblem =>
            problem => new DeleteProblemViewModel
            {
                Id = problem.Id,
                ContestId = problem.ContestId,
                Name = problem.Name,
                MaximumPoints = problem.MaximumPoints,
                TimeLimit = problem.TimeLimit,
                MemoryLimit = problem.MemoryLimit,
                SourceCodeSizeLimit = problem.SourceCodeSizeLimit,
                Checker = problem.Checker.Name,
                OrderBy = problem.OrderBy,
                SolutionSkeletonData = problem.SolutionSkeleton
            };

        public int Id { get; set; }

        public int ContestId { get; set; }

        public string Name { get; set; }

        [Display(Name = "Max_points", ResourceType = typeof(Resource))]
        public short MaximumPoints { get; set; }

        [Display(Name = "Time_limit", ResourceType = typeof(Resource))]
        public int TimeLimit { get; set; }

        [Display(Name = "Memory_limit", ResourceType = typeof(Resource))]
        public int MemoryLimit { get; set; }

        [Display(Name = "Source_code_size_limit", ResourceType = typeof(Resource))]
        public int? SourceCodeSizeLimit { get; set; }

        public string Checker { get; set; }

        [Display(Name = "Order", ResourceType = typeof(Resource))]
        public int OrderBy { get; set; }

        [AllowHtml]
        [Display(Name = "Solution_skeleton", ResourceType = typeof(Resource))]
        [UIHint("MultiLineText")]
        public string SolutionSkeletonShort
        {
            get
            {
                if (this.SolutionSkeletonData == null)
                {
                    return null;
                }

                var solutionSkeleton = this.SolutionSkeletonData.Decompress();
                return solutionSkeleton.Length > 200
                    ? $"{solutionSkeleton.Substring(0, 200)}..."
                    : solutionSkeleton;
            }
        }

        [AllowHtml]
        private byte[] SolutionSkeletonData { get; set; }
    }
}