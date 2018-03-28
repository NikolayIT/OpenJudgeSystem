namespace OJS.Web.Areas.Administration.ViewModels.Problem
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    using Resource = Resources.Areas.Administration.Problems.ViewModels.DetailedProblem;

    public class DeleteProblemViewModel
    {
        public static Expression<Func<Problem, DeleteProblemViewModel>> FromProblem =>
            problem => new DeleteProblemViewModel
            {
                Id = problem.Id,
                ContestId = problem.ProblemGroup.ContestId,
                Name = problem.Name,
                ContestName = problem.ProblemGroup.Contest.Name,
                OrderBy = problem.OrderBy
            };

        public int Id { get; set; }

        public int ContestId { get; set; }

        [Display(Name = "Name", ResourceType = typeof(Resource))]
        public string Name { get; set; }

        [Display(Name = "Contest", ResourceType = typeof(Resource))]
        public string ContestName { get; set; }

        [Display(Name = "Order", ResourceType = typeof(Resource))]
        public int OrderBy { get; set; }
    }
}