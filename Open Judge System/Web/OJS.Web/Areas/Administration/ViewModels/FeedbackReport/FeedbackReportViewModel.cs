namespace OJS.Web.Areas.Administration.ViewModels.FeedbackReport
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    using Resource = Resources.Areas.Administration.Feedback.ViewModels.FeedbackReport;

    public class FeedbackReportViewModel : AdministrationViewModel<FeedbackReport>
    {
        [ExcludeFromExcel]
        public static Expression<Func<FeedbackReport, FeedbackReportViewModel>> FromFeedbackReport
        {
            get
            {
                return feedback => new FeedbackReportViewModel
                {
                    Id = feedback.Id,
                    Name = feedback.Name,
                    Email = feedback.Email,
                    Content = feedback.Content,
                    Username = feedback.User == null ? Resource.Anonymous : feedback.User.UserName,
                    IsFixed = feedback.IsFixed,
                    CreatedOn = feedback.CreatedOn,
                    ModifiedOn = feedback.ModifiedOn
                };
            }
        }

        [DatabaseProperty]
        [Display(Name = "№")]
        [DefaultValue(null)]
        [HiddenInput(DisplayValue = false)]
        public int? Id { get; set; }

        [DatabaseProperty]
        [Display(Name = "Name", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Name_required",
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint("SingleLineText")]
        public string Name { get; set; }

        [DatabaseProperty]
        [Display(Name = "Mail", ResourceType = typeof(Resource))]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(
            GlobalConstants.EmailRegEx,
            ErrorMessageResourceName = "Mail_invalid",
            ErrorMessageResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Mail_required",
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint("SingleLineText")]
        public string Email { get; set; }

        [DatabaseProperty]
        [Display(Name = "Contet", ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = "Contet_required",
            ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.MultilineText)]
        [UIHint("MultiLineText")]
        public string Content { get; set; }

        [Display(Name = "UserName", ResourceType = typeof(Resource))]
        [UIHint("NonEditable")]
        public string Username { get; set; }

        [DatabaseProperty]
        [Display(Name = "Is_fixed", ResourceType = typeof(Resource))]
        public bool IsFixed { get; set; }
    }
}