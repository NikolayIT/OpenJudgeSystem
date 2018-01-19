namespace OJS.Web.Areas.Administration.ViewModels.Problem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.SqlServer;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Common.DataAnnotations;
    using OJS.Common.Extensions;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;
    using OJS.Web.Areas.Administration.ViewModels.ProblemResource;
    using OJS.Web.Areas.Administration.ViewModels.SubmissionType;

    using Resources.Areas.Administration.Contests.ViewModels;
    using Resource = Resources.Areas.Administration.Problems.ViewModels.DetailedProblem;

    public class DetailedProblemViewModel : AdministrationViewModel<Problem>
    {
        public DetailedProblemViewModel()
        {
            this.Name = GlobalConstants.ProblemDefaultName;
            this.MaximumPoints = GlobalConstants.ProblemDefaultMaximumPoints;
            this.TimeLimit = GlobalConstants.ProblemDefaultTimeLimit;
            this.MemoryLimit = GlobalConstants.ProblemDefaultMemoryLimit;
            this.ShowResults = GlobalConstants.ProblemDefaultShowResults;
            this.SourceCodeSizeLimit = GlobalConstants.ProblemDefaultSourceLimit;
            this.ShowDetailedFeedback = GlobalConstants.ProblemDefaultShowDetailedFeedback;
            this.SubmissionTypes = new List<SubmissionTypeViewModel>();
        }

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
                    SelectedSubmissionTypes = problem.SubmissionTypes.AsQueryable().Select(SubmissionTypeViewModel.ViewModel),
                    ShowResults = problem.ShowResults,
                    ShowDetailedFeedback = problem.ShowDetailedFeedback,
                    SourceCodeSizeLimit = problem.SourceCodeSizeLimit,
                    Checker = problem.Checker.Name,
                    OrderBy = problem.OrderBy,
                    ProblemGroupOrderBy = problem.ProblemGroup.OrderBy + 1,
                    SolutionSkeletonData = problem.SolutionSkeleton,
                    HasAdditionalFiles = problem.AdditionalFiles != null && SqlFunctions.DataLength(problem.AdditionalFiles) > 0,
                    CreatedOn = problem.CreatedOn,
                    ModifiedOn = problem.ModifiedOn,
                };
            }
        }

        [DatabaseProperty]
        public int Id { get; set; }

        [DatabaseProperty]
        [Display(Name = "Name", ResourceType = typeof(Resource))]
        [Required(
            AllowEmptyStrings = false,
            ErrorMessageResourceName = "Name_required",
            ErrorMessageResourceType = typeof(Resource))]
        [MaxLength(
            GlobalConstants.ProblemNameMaxLength,
            ErrorMessageResourceName = "Name_length",
            ErrorMessageResourceType = typeof(Resource))]
        [DefaultValue(GlobalConstants.ProblemDefaultName)]
        public string Name { get; set; }

        [DatabaseProperty]
        [Display(Name = "Contest", ResourceType = typeof(Resource))]
        public int ContestId { get; set; }

        [Display(Name = "Contest", ResourceType = typeof(Resource))]
        public string ContestName { get; set; }

        [Display(Name = "Trial_tests", ResourceType = typeof(Resource))]
        public int TrialTests { get; set; }

        [Display(Name = "Compete_tests", ResourceType = typeof(Resource))]
        public int CompeteTests { get; set; }

        [DatabaseProperty]
        [Display(Name = "Max_points", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Max_points_required",
            ErrorMessageResourceType = typeof(Resource))]
        [DefaultValue(GlobalConstants.ProblemDefaultMaximumPoints)]
        public short MaximumPoints { get; set; }

        [DatabaseProperty]
        [Display(Name = "Time_limit", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Time_limit_required",
            ErrorMessageResourceType = typeof(Resource))]
        [DefaultValue(GlobalConstants.ProblemDefaultTimeLimit)]
        public int TimeLimit { get; set; }

        [DatabaseProperty]
        [Display(Name = "Memory_limit", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Memory_limit_required",
            ErrorMessageResourceType = typeof(Resource))]
        [DefaultValue(GlobalConstants.ProblemDefaultMemoryLimit)]
        public int MemoryLimit { get; set; }

        [Display(Name = "Checker", ResourceType = typeof(Resource))]
        public string Checker { get; set; }

        [ExcludeFromExcel]
        [UIHint("FileUpload")]
        public HttpPostedFileBase AdditionalFiles { get; set; }

        [ExcludeFromExcel]
        public IEnumerable<SelectListItem> AvailableCheckers { get; set; }

        [ExcludeFromExcel]
        public bool HasAdditionalFiles { get; set; }

        [DatabaseProperty]
        [Display(Name = "Order", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Order_required",
            ErrorMessageResourceType = typeof(Resource))]
        [DefaultValue(0)]
        public int OrderBy { get; set; }

        [DatabaseProperty]
        [Display(Name = "Group_number", ResourceType = typeof(Resource))]
        [UIHint("DropDownList")]
        public int? ProblemGroupOrderBy { get; set; }

        [DatabaseProperty]
        [Display(Name = "Source_code_size_limit", ResourceType = typeof(Resource))]
        [DefaultValue(GlobalConstants.ProblemDefaultSourceLimit)]
        public int? SourceCodeSizeLimit { get; set; }

        [DatabaseProperty]
        [Display(Name = "Show_results", ResourceType = typeof(Resource))]
        [DefaultValue(GlobalConstants.ProblemDefaultShowResults)]
        public bool ShowResults { get; set; }

        [DatabaseProperty]
        [Display(Name = "Show_detailed_feedback", ResourceType = typeof(Resource))]
        [DefaultValue(GlobalConstants.ProblemDefaultShowDetailedFeedback)]
        public bool ShowDetailedFeedback { get; set; }

        [Display(Name = "Submision_types", ResourceType = typeof(ContestAdministration))]
        [ExcludeFromExcel]
        public IList<SubmissionTypeViewModel> SubmissionTypes { get; set; }

        [ExcludeFromExcel]
        public IEnumerable<SubmissionTypeViewModel> SelectedSubmissionTypes { get; set; }

        [ExcludeFromExcel]
        public IEnumerable<ProblemResourceViewModel> Resources { get; set; }

        [AllowHtml]
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

        [AllowHtml]
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

        [ExcludeFromExcel]
        [UIHint("FileUpload")]
        public HttpPostedFileBase Tests { get; set; }

        [AllowHtml]
        internal byte[] SolutionSkeletonData { get; set; }
    }
}