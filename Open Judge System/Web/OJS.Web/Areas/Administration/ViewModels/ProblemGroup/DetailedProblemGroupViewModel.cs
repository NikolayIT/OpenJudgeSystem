namespace OJS.Web.Areas.Administration.ViewModels.ProblemGroup
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    using Resource = Resources.Areas.Administration.Problems.ViewModels.DetailedProblem;

    public class DetailedProblemGroupViewModel : AdministrationViewModel<ProblemGroup>
    {
        public static Expression<Func<ProblemGroup, DetailedProblemGroupViewModel>> FromProblemGroup =>
            problemGroup => new DetailedProblemGroupViewModel
            {
                Id = problemGroup.Id,
                OrderBy = problemGroup.OrderBy,
                ContestId = problemGroup.ContestId,
                ContestName = problemGroup.Contest.Name
            };

        [DatabaseProperty]
        [Display(Name = "№")]
        public int Id { get; set; }

        [DatabaseProperty]
        [Display(Name = "Order", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Order_required",
            ErrorMessageResourceType = typeof(Resource))]
        public int OrderBy { get; set; }

        [DatabaseProperty]
        [Display(Name = "Contest", ResourceType = typeof(Resource))]
        public int ContestId { get; set; }

        [Display(Name = "Contest", ResourceType = typeof(Resource))]
        public string ContestName { get; set; }
    }
}