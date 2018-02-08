namespace OJS.Web.Areas.Administration.ViewModels.ProblemGroup
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    using ContestResource = Resources.Areas.Administration.Contests.ViewModels.ContestAdministration;
    using ProblemResource = Resources.Areas.Administration.Problems.ViewModels.DetailedProblem;

    public class DetailedProblemGroupViewModel : AdministrationViewModel<ProblemGroup>
    {
        public static Expression<Func<ProblemGroup, DetailedProblemGroupViewModel>> FromProblemGroup =>
            problemGroup => new DetailedProblemGroupViewModel
            {
                Id = problemGroup.Id,
                OrderBy = problemGroup.OrderBy,
                ContestId = problemGroup.ContestId,
                ContestName = problemGroup.Contest.Name,
                CategoryName = problemGroup.Contest.Category.Name
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

        [Display(Name = "Category", ResourceType = typeof(ContestResource))]
        [HiddenInput(DisplayValue = false)]
        public string CategoryName { get; set; }
    }
}