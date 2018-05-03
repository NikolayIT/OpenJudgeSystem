namespace OJS.Web.Areas.Administration.ViewModels.ProblemGroup
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    using ProblemResource = Resources.Areas.Administration.Problems.ViewModels.DetailedProblem;

    public class ProblemGroupAdministrationViewModel : AdministrationViewModel<ProblemGroup>
    {
        public static Expression<Func<ProblemGroup, ProblemGroupAdministrationViewModel>> FromProblemGroup =>
            problemGroup => new ProblemGroupAdministrationViewModel
            {
                Id = problemGroup.Id,
                OrderBy = problemGroup.OrderBy,
                ContestId = problemGroup.ContestId,
                ContestName = problemGroup.Contest.Name
            };

        [DatabaseProperty]
        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [DatabaseProperty]
        [Display(Name = "Order", ResourceType = typeof(ProblemResource))]
        [Required(
            ErrorMessageResourceName = "Order_required",
            ErrorMessageResourceType = typeof(ProblemResource))]
        [UIHint("PositiveInteger")]
        public int OrderBy { get; set; }

        [DatabaseProperty]
        [HiddenInput(DisplayValue = false)]
        public int ContestId { get; set; }

        [Display(Name = "Contest", ResourceType = typeof(ProblemResource))]
        [UIHint("NonEditable")]
        public string ContestName { get; set; }
    }
}