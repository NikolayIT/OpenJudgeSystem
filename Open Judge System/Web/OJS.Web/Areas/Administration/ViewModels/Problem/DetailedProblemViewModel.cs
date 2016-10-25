namespace OJS.Web.Areas.Administration.ViewModels.Problem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Common.DataAnnotations;
    using OJS.Common.Extensions;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.ProblemResource;

    using Resource = Resources.Areas.Administration.Problems.ViewModels.DetailedProblem;

    public class DetailedProblemViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<Problem, DetailedProblemViewModel>> FromProblem
        {
            get
            {
                return problem => new DetailedProblemViewModel
                {
                    Id = problem.Id,
                    Name = problem.Name,
                    ContestId = problem.ContestId,
                    ContestName = problem.Contest.Name,
                    TrialTests = problem.Tests.AsQueryable().Count(x => x.IsTrialTest),
                    CompeteTests = problem.Tests.AsQueryable().Count(x => !x.IsTrialTest),
                    MaximumPoints = problem.MaximumPoints,
                    TimeLimit = problem.TimeLimit,
                    MemoryLimit = problem.MemoryLimit,
                    ShowResults = problem.ShowResults,
                    ShowDetailedFeedback = problem.ShowDetailedFeedback,
                    SourceCodeSizeLimit = problem.SourceCodeSizeLimit,
                    Checker = problem.Checker.Name,
                    OrderBy = problem.OrderBy,
                    SolutionSkeletonData = problem.SolutionSkeleton
                };
            }
        }

        public int Id { get; set; }

        [Display(Name = "Name", ResourceType = typeof(Resource))]
        [Required(
            AllowEmptyStrings = false,
            ErrorMessageResourceName = "Name_required",
            ErrorMessageResourceType = typeof(Resource))]
        [MaxLength(
            GlobalConstants.ProblemNameMaxLength,
            ErrorMessageResourceName = "Name_length",
            ErrorMessageResourceType = typeof(Resource))]
        [DefaultValue("Име")]
        public string Name { get; set; }

        [Display(Name = "Contest", ResourceType = typeof(Resource))]
        public int ContestId { get; set; }

        [Display(Name = "Contest", ResourceType = typeof(Resource))]
        public string ContestName { get; set; }

        [Display(Name = "Trial_tests", ResourceType = typeof(Resource))]
        public int TrialTests { get; set; }

        [Display(Name = "Compete_tests", ResourceType = typeof(Resource))]
        public int CompeteTests { get; set; }

        [Display(Name = "Max_points", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Max_points_required",
            ErrorMessageResourceType = typeof(Resource))]
        [DefaultValue(GlobalConstants.ProblemDefaultMaximumPoints)]
        public short MaximumPoints { get; set; }

        [Display(Name = "Time_limit", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Time_limit_required",
            ErrorMessageResourceType = typeof(Resource))]
        [DefaultValue(GlobalConstants.ProblemDefaultTimeLimit)]
        public int TimeLimit { get; set; }

        [Display(Name = "Memory_limit", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Memory_limit_required",
            ErrorMessageResourceType = typeof(Resource))]
        [DefaultValue(GlobalConstants.ProblemDefaultMemoryLimit)]
        public int MemoryLimit { get; set; }

        [Display(Name = "Checker", ResourceType = typeof(Resource))]
        public string Checker { get; set; }

        [ExcludeFromExcel]
        public IEnumerable<SelectListItem> AvailableCheckers { get; set; }

        [Display(Name = "Order", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Order_required",
            ErrorMessageResourceType = typeof(Resource))]
        [DefaultValue(0)]
        public int OrderBy { get; set; }

        [Display(Name = "Source_code_size_limit", ResourceType = typeof(Resource))]
        [DefaultValue(null)]
        public int? SourceCodeSizeLimit { get; set; }

        [Display(Name = "Show_results", ResourceType = typeof(Resource))]
        public bool ShowResults { get; set; }

        [Display(Name = "Show_detailed_feedback", ResourceType = typeof(Resource))]
        public bool ShowDetailedFeedback { get; set; }

        [ExcludeFromExcel]
        public IEnumerable<ProblemResourceViewModel> Resources { get; set; }

        [Display(Name = "Solution_skeleton", ResourceType = typeof(Resource))]
        [UIHint("MultiLineText")]
        public string SolutionSkeleton
        {
            get
            {
                return this.SolutionSkeletonData.Decompress();
            }

            set
            {
                this.SolutionSkeletonData = !string.IsNullOrWhiteSpace(value) ? value.Compress() : null;
            }
        }

        public string SolutionSkeletonShort
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.SolutionSkeleton))
                {
                    return null;
                }

                return this.SolutionSkeleton.Length > 200
                    ? this.SolutionSkeleton.Substring(0, 200)
                    : this.SolutionSkeleton;
            }
        }

        internal byte[] SolutionSkeletonData { get; set; }
    }
}