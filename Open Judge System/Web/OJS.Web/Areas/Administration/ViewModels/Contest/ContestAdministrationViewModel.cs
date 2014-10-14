namespace OJS.Web.Areas.Administration.ViewModels.Contest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common.DataAnnotations;
    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;
    using OJS.Web.Areas.Administration.ViewModels.SubmissionType;

    public class ContestAdministrationViewModel : AdministrationViewModel<Contest>
    {
        public ContestAdministrationViewModel()
        {
            this.SubmisstionTypes = new List<SubmissionTypeViewModel>();
        }

        [ExcludeFromExcel]
        public static Expression<Func<Contest, ContestAdministrationViewModel>> ViewModel
        {
            get
            {
                return contest => new ContestAdministrationViewModel
                {
                    Id = contest.Id,
                    Name = contest.Name,
                    Type = (int)contest.Type,
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
                    SelectedSubmissionTypes = contest.SubmissionTypes.AsQueryable().Select(SubmissionTypeViewModel.ViewModel),
                    CreatedOn = contest.CreatedOn,
                    ModifiedOn = contest.ModifiedOn,
                };
            }
        }

        [DatabaseProperty]
        [Display(Name = "№")]
        [DefaultValue(null)]
        [HiddenInput(DisplayValue = false)]
        public int? Id { get; set; }

        [DatabaseProperty]
        [Display(Name = "Име")]
        [Required(ErrorMessage = "Името е задължително!")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Позволената дължина е между 6 и 100 символа")]
        [UIHint("SingleLineText")]
        public string Name { get; set; }

        [DatabaseProperty]
        [Display(Name = "Тип")]
        [UIHint("DropDownListCustom")]
        public int Type { get; set; }

        [DatabaseProperty]
        [Display(Name = "Начало")]
        [UIHint("DateAndTime")]
        public DateTime? StartTime { get; set; }

        [DatabaseProperty]
        [Display(Name = "Край")]
        [UIHint("DateAndTime")]
        public DateTime? EndTime { get; set; }

        [DatabaseProperty]
        [Display(Name = "Начало упражнение")]
        [UIHint("DateAndTime")]
        public DateTime? PracticeStartTime { get; set; }

        [DatabaseProperty]
        [Display(Name = "Край упражнение")]
        [UIHint("DateAndTime")]
        public DateTime? PracticeEndTime { get; set; }

        [DatabaseProperty]
        [Display(Name = "Парола за състезание")]
        [UIHint("SingleLineText")]
        public string ContestPassword { get; set; }

        [DatabaseProperty]
        [Display(Name = "Парола за упражнение")]
        [UIHint("SingleLineText")]
        public string PracticePassword { get; set; }

        [DatabaseProperty]
        [Display(Name = "Описание")]
        [UIHint("MultiLineText")]
        public string Description { get; set; }

        [DatabaseProperty]
        [Display(Name = "Време между събмисии")]
        [UIHint("PositiveInteger")]
        [DefaultValue(0)]
        public int LimitBetweenSubmissions { get; set; }

        [DatabaseProperty]
        [Display(Name = "Подредба")]
        [Required(ErrorMessage = "Подредбата е задължителна!")]
        [UIHint("Integer")]
        public int OrderBy { get; set; }

        [DatabaseProperty]
        [Display(Name = "Видимост")]
        public bool IsVisible { get; set; }

        [DatabaseProperty]
        [Display(Name = "Категория")]
        [Required(ErrorMessage = "Категорията е задължителна!")]
        [UIHint("CategoryDropDown")]
        [DefaultValue(null)]
        public int? CategoryId { get; set; }

        [Display(Name = "Тип решения")]
        [ExcludeFromExcel]
        [UIHint("SubmissionTypeCheckBoxes")]
        public IList<SubmissionTypeViewModel> SubmisstionTypes { get; set; }

        [ExcludeFromExcel]
        public IEnumerable<SubmissionTypeViewModel> SelectedSubmissionTypes { get; set; }

        public override Contest GetEntityModel(Contest model = null)
        {
            model = model ?? new Contest();

            model.Type = (ContestType)this.Type;

            return base.GetEntityModel(model);
        }
    }
}