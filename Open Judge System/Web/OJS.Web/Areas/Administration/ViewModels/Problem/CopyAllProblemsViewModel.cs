namespace OJS.Web.Areas.Administration.ViewModels.Problem
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using Resources.Areas.Administration.Problems.ViewModels;

    using static OJS.Common.Constants.EditorTemplateConstants;

    using Resource = Resources.Areas.Administration.Problems.ViewModels.CopyProblem;

    public class CopyAllProblemsViewModel
    {
        // Needed for binding the model to the action
        public CopyAllProblemsViewModel()
        {
        }

        public CopyAllProblemsViewModel(int contestId) => this.FromContestId = contestId;

        [Required]
        [HiddenInput(DisplayValue = false)]
        public int FromContestId { get; set; }

        [Display(Name = nameof(Resource.Bulk_copy_contest_label), ResourceType = typeof(CopyProblem))]
        [Required(
            ErrorMessageResourceName = nameof(CopyProblem.Contest_required),
            ErrorMessageResourceType = typeof(CopyProblem))]
        [DefaultValue(null)]
        [UIHint(ContestsComboBox)]
        public int? ContestId { get; set; }
    }
}