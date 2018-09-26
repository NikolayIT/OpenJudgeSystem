namespace OJS.Web.Areas.Administration.ViewModels.Contest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Common.DataAnnotations;
    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;
    using OJS.Web.Common;

    using static OJS.Common.Constants.EditorTemplateConstants;

    using Resource = Resources.Areas.Administration.Contests.ViewModels.ContestAdministration;

    public class ContestAdministrationViewModel : AdministrationViewModel<Contest>
    {
        private IEnumerable<IpAdministrationViewModel> allowedIps;

        public ContestAdministrationViewModel() =>
            this.allowedIps = new List<IpAdministrationViewModel>();

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
                    ProblemGroupsCount = contest.ProblemGroups.Count(pg => !pg.IsDeleted),
                    StartTime = contest.StartTime,
                    EndTime = contest.EndTime,
                    PracticeStartTime = contest.PracticeStartTime,
                    PracticeEndTime = contest.PracticeEndTime,
                    Duration = contest.Duration,
                    IsVisible = contest.IsVisible,
                    CategoryId = contest.CategoryId.Value,
                    CategoryName = contest.Category.Name,
                    ContestPassword = contest.ContestPassword,
                    PracticePassword = contest.PracticePassword,
                    NewIpPassword = contest.NewIpPassword,
                    allowedIps = contest.AllowedIps
                        .Where(y => y.IsOriginallyAllowed)
                        .Select(y => y.Ip)
                        .AsQueryable()
                        .Select(IpAdministrationViewModel.ViewModel),
                    Description = contest.Description,
                    LimitBetweenSubmissions = contest.LimitBetweenSubmissions,
                    OrderBy = contest.OrderBy,
                    CreatedOn = contest.CreatedOn,
                    ModifiedOn = contest.ModifiedOn,
                    AutoChangeTestsFeedbackVisibility = contest.AutoChangeTestsFeedbackVisibility,
                };
            }
        }

        [DatabaseProperty]
        [Display(Name = "№")]
        [DefaultValue(null)]
        [HiddenInput(DisplayValue = false)]
        public int? Id { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Name), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Name_required),
            ErrorMessageResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.ContestNameMaxLength,
            MinimumLength = GlobalConstants.ContestNameMinLength,
            ErrorMessageResourceName = nameof(Resource.Name_length),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint(SingleLineText)]
        public string Name { get; set; }

        [DatabaseProperty]
        [Display(Name = "Тип")]
        [UIHint(KendoDropDownList)]
        public int Type { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Number_of_problem_groups), ResourceType = typeof(Resource))]
        [UIHint(KendoPositiveInteger)]
        [DefaultValue(0)]
        public int ProblemGroupsCount { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Start_time), ResourceType = typeof(Resource))]
        [UIHint(KendoDateAndTimePicker)]
        public DateTime? StartTime { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.End_time), ResourceType = typeof(Resource))]
        [UIHint(KendoDateAndTimePicker)]
        public DateTime? EndTime { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Practice_start_time), ResourceType = typeof(Resource))]
        [UIHint(KendoDateAndTimePicker)]
        public DateTime? PracticeStartTime { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Practice_end_time), ResourceType = typeof(Resource))]
        [UIHint(KendoDateAndTimePicker)]
        public DateTime? PracticeEndTime { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Duration), ResourceType = typeof(Resource))]
        [AdditionalMetadata(WebConstants.Placeholder, "hh:mm")]
        [UIHint(KendoTimePicker)]
        public TimeSpan? Duration { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Contest_password), ResourceType = typeof(Resource))]
        [MaxLength(
            GlobalConstants.ContestPasswordMaxLength,
            ErrorMessageResourceName = nameof(Resource.Password_max_length),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint(SingleLineText)]
        public string ContestPassword { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Practice_password), ResourceType = typeof(Resource))]
        [MaxLength(
            GlobalConstants.ContestPasswordMaxLength,
            ErrorMessageResourceName = nameof(Resource.Password_max_length),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint(SingleLineText)]
        public string PracticePassword { get; set; }

        [DatabaseProperty]
        [Display(Name = "Парола за ново IP")]
        [UIHint(SingleLineText)]
        public string NewIpPassword { get; set; }

        [DatabaseProperty]
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        [Display(Name = nameof(Resource.Description), ResourceType = typeof(Resource))]
        [UIHint(HtmlContent)]
        public string Description { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Submissions_limit), ResourceType = typeof(Resource))]
        [UIHint(KendoPositiveInteger)]
        [DefaultValue(0)]
        public int LimitBetweenSubmissions { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Order), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Order_required),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint(KendoInteger)]
        public int OrderBy { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Visibility), ResourceType = typeof(Resource))]
        public bool IsVisible { get; set; }

        [DatabaseProperty]
        [Display(
            Name = nameof(Resource.Auto_change_tests_feedback_visibility),
            ResourceType = typeof(Resource))]
        public bool AutoChangeTestsFeedbackVisibility { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Category), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Category_required),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint(CategoryDropDown)]
        [DefaultValue(null)]
        public int? CategoryId { get; set; }

        [Display(Name = nameof(Resource.Category_name), ResourceType = typeof(Resource))]
        [ExcludeFromExcel]
        [UIHint(SingleLineText)]
        public string CategoryName { get; set; }

        [Display(Name = "Позволени IP-та")]
        public string AllowedIps { get; set; }

        public string RawAllowedIps
        {
            get { return string.Join(", ", this.allowedIps.Select(x => x.Value)); }
        }

        public bool IsOnline => this.Type == (int)ContestType.OnlinePracticalExam;

        public override Contest GetEntityModel(Contest model = null)
        {
            model = model ?? new Contest();

            model.Type = (ContestType)this.Type;

            return base.GetEntityModel(model);
        }
    }
}