namespace OJS.Web.Areas.Administration.ViewModels.Problem
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    using static OJS.Common.Constants.EditorTemplateConstants;

    using Resource = Resources.Areas.Administration.Problems.ViewModels.CopyProblem;

    public class CopyProblemViewModel
    {
        public CopyProblemViewModel()
        {
        }

        public CopyProblemViewModel(int problemId) => this.Id = problemId;

        [Required]
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [Display(Name = nameof(Resource.Contest_label), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Contest_required),
            ErrorMessageResourceType = typeof(Resource))]
        [DefaultValue(null)]
        [UIHint(ContestsComboBox)]
        public int? ContestId { get; set; }

        [Display(Name = nameof(Resource.Problem_group_label), ResourceType = typeof(Resource))]
        [UIHint(ProblemGroupsCascadeDropDown)]
        public int? ProblemGroupId { get; set; }
    }
}