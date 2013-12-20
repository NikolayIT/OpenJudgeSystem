namespace OJS.Web.Areas.Administration.ViewModels.Contest
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;
    using OJS.Web.Areas.Administration.ViewModels.SubmissionType;
    using System.Collections.Generic;

    public class ContestAdministrationViewModel : AdministrationViewModel
    {
        [ExcludeFromExcelAttribute]
        public static Expression<Func<Contest, ContestAdministrationViewModel>> ViewModel
        {
            get
            {
                return contest => new ContestAdministrationViewModel
                {
                    Id = contest.Id,
                    Name = contest.Name,
                    StartTime = contest.StartTime,
                    EndTime = contest.EndTime,
                    PracticeStartTime = contest.PracticeStartTime,
                    PracticeEndTime = contest.PracticeEndTime,
                    IsVisible = contest.IsVisible,
                    CategoryId = contest.CategoryId.Value,
                    ContestPassword = contest.ContestPassword,
                    PracticePassword = contest.PracticePassword,
                    Description = contest.Description,
                    LimitBetweenSubmissions = contest.LimitBetweenSubmissions,
                    OrderBy = contest.OrderBy,
                    CreatedOn = contest.CreatedOn,
                    ModifiedOn = contest.ModifiedOn,
                };
            }
        }

        [ExcludeFromExcelAttribute]
        public Contest ToEntity
        {
            get
            {
                return new Contest
                {
                    Id = this.Id ?? default(int),
                    Name = this.Name,
                    StartTime = this.StartTime,
                    EndTime = this.EndTime,
                    PracticeStartTime = this.PracticeStartTime,
                    PracticeEndTime = this.PracticeEndTime,
                    IsVisible = this.IsVisible,
                    CategoryId = this.CategoryId,
                    ContestPassword = this.ContestPassword,
                    PracticePassword = this.PracticePassword,
                    Description = this.Description,
                    LimitBetweenSubmissions = this.LimitBetweenSubmissions,
                    OrderBy = this.OrderBy,
                    CreatedOn = this.CreatedOn.GetValueOrDefault(),
                    ModifiedOn = this.ModifiedOn,
                };
            }
        }

        public ContestAdministrationViewModel()
        {
            this.SubmisstionTypes = new List<SubmissionTypeViewModel>();
        }

        [Display(Name = "№")]
        [DefaultValue(null)]
        [HiddenInput(DisplayValue = false)]
        public int? Id { get; set; }

        [Display(Name = "Име")]
        [Required(ErrorMessage = "Името е задължително!")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Позволената дължина е между 6 и 100 символа")]
        [UIHint("SingleLineText")]
        public string Name { get; set; }

        [Display(Name = "Начало")]
        [UIHint("DateAndTime")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "Край")]
        [UIHint("DateAndTime")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Начало упражнение")]
        [UIHint("DateAndTime")]
        public DateTime? PracticeStartTime { get; set; }

        [Display(Name = "Край упражнение")]
        [UIHint("DateAndTime")]
        public DateTime? PracticeEndTime { get; set; }

        [Display(Name = "Парола за състезание")]
        [UIHint("SingleLineText")]
        public string ContestPassword { get; set; }

        [Display(Name = "Парола за упражнение")]
        [UIHint("SingleLineText")]
        public string PracticePassword { get; set; }

        [Display(Name = "Описание")]
        [UIHint("MultiLineText")]
        public string Description { get; set; }

        [Display(Name = "Време между събмисии")]
        [UIHint("PositiveInteger")]
        [DefaultValue(0)]
        public int LimitBetweenSubmissions { get; set; }

        [Display(Name = "Подредба")]
        [Required(ErrorMessage = "Подредбата е задължителна!")]
        [UIHint("PositiveInteger")]
        public int OrderBy { get; set; }

        [Display(Name = "Видимост")]
        public bool IsVisible { get; set; }

        [Display(Name = "Категория")]
        [Required(ErrorMessage = "Категорията е задължителна!")]
        [UIHint("CategoryDropDown")]
        [DefaultValue(null)]
        public int? CategoryId { get; set; }

        [Display(Name = "Тип решения")]
        [UIHint("SubmissionTypeCheckBoxes")]
        public IList<SubmissionTypeViewModel> SubmisstionTypes { get; set; }
    }
}