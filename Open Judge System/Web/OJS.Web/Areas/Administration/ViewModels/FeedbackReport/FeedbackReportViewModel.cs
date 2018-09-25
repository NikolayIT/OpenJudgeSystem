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

    using static OJS.Common.Constants.EditorTemplateConstants;

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
        [Display(Name = nameof(Resource.Name), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Name_required),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint(SingleLineText)]
        public string Name { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Mail), ResourceType = typeof(Resource))]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(
            GlobalConstants.EmailRegEx,
            ErrorMessageResourceName = nameof(Resource.Mail_invalid),
            ErrorMessageResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Mail_required),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint(SingleLineText)]
        public string Email { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Contet), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Contet_required),
            ErrorMessageResourceType = typeof(Resource))]
        [DataType(DataType.MultilineText)]
        [UIHint(MultiLineText)]
        public string Content { get; set; }

        [Display(Name = nameof(Resource.UserName), ResourceType = typeof(Resource))]
        [UIHint(NonEditable)]
        public string Username { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Is_fixed), ResourceType = typeof(Resource))]
        public bool IsFixed { get; set; }
    }
}