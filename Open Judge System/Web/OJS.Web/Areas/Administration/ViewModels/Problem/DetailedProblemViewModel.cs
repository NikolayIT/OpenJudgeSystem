namespace OJS.Web.Areas.Administration.ViewModels.Problem
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.ProblemResource;

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
                    OrderBy = problem.OrderBy
                };
            }
        }

        public int Id { get; set; }

        [Display(Name = "Име")]
        [Required(ErrorMessage = "Името е задължително!", AllowEmptyStrings = false)]
        [MaxLength(50, ErrorMessage = "Максималната дължина е 50 символа!")]
        [DefaultValue("Име")]
        public string Name { get; set; }

        public int ContestId { get; set; }

        [Display(Name = "Състезание")]
        public string ContestName { get; set; }

        [Display(Name = "Пробни тестове")]
        public int TrialTests { get; set; }

        [Display(Name = "Състезателни тестове")]
        public int CompeteTests { get; set; }

        [Display(Name = "Максимум точки")]
        [Required(ErrorMessage = "Максимум точките са задължителни!")]
        [DefaultValue(100)]
        public short MaximumPoints { get; set; }

        [Display(Name = "Лимит на време")]
        [Required(ErrorMessage = "Лимита на време е задължителен!")]
        [DefaultValue(1000)]
        public int TimeLimit { get; set; }

        [Display(Name = "Лимит на памет")]
        [Required(ErrorMessage = "Лимита на памет е задължителен!")]
        [DefaultValue(32 * 1024 * 1024)]
        public int MemoryLimit { get; set; }
        
        [Display(Name = "Чекер")]
        public string Checker { get; set; }

        [ExcludeFromExcel]
        public IEnumerable<SelectListItem> AvailableCheckers { get; set; }

        [Display(Name = "Подредба")]
        [Required(ErrorMessage = "Подредбата е задължителна!")]
        [DefaultValue(0)]
        public int OrderBy { get; set; }

        [Display(Name = "Размер на сорс кода")]
        [DefaultValue(null)]
        public int? SourceCodeSizeLimit { get; set; }

        [Display(Name = "Видими резултати")]
        public bool ShowResults { get; set; }

        [Display(Name = "Покажи пълния feedback")]
        public bool ShowDetailedFeedback { get; set; }

        [ExcludeFromExcel]
        public IEnumerable<ProblemResourceViewModel> Resources { get; set; }
    }
}