namespace OJS.Web.Areas.Administration.ViewModels.Problem
{
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using Resource = Resources.Areas.Administration.Problems.ViewModels.CopyProblem;

    public class CopyProblemViewModel
    {
        public CopyProblemViewModel(int problemId) => this.Id = problemId;

        [Required]
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [Display(Name = "Contest_label", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Contest_required",
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint("ContestsCopyToComboBox")]
        public int ContestToCopyTo { get; set; }

        [Display(Name = "Problem_group_label", ResourceType = typeof(Resource))]
        [UIHint("ProblemGroupsCascadeDropDown")]
        public int? ProblemGroupToCopyTo { get; set; }
    }
}