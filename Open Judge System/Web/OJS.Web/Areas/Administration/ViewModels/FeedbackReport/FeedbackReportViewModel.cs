namespace OJS.Web.Areas.Administration.ViewModels.FeedbackReport
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;
    using System.ComponentModel;
    using OJS.Common.DataAnnotations;

    public class FeedbackReportViewModel : AdministrationViewModel
    {
        [ExcludeFromExcelAttribute]
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
                    Username = feedback.User == null ? "Anonymous" : feedback.User.UserName,
                    IsFixed = feedback.IsFixed,
                    CreatedOn = feedback.CreatedOn,
                    ModifiedOn = feedback.ModifiedOn
                };
            }
        }

        [Display(Name = "№")]
        [DefaultValue(null)]
        [UIHint("NonEditable")]
        public int? Id { get; set; }

        [Display(Name = "Име")]
        [Required(ErrorMessage = "Името е задължително")]
        [UIHint("SingleLineText")]
        public string Name { get; set; }

        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "Имейла е задължителен")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Невалиден имейл адрес")]
        public string Email { get; set; }

        [Display(Name = "Съдържание")]
        [Required(ErrorMessage = "Съдържанието е задължително")]
        [DataType(DataType.MultilineText)]
        [UIHint("MultiLineText")]
        public string Content { get; set; }

        [Display(Name = "Потребител")]
        [UIHint("SingleLineText")]
        public string Username { get; set; }

        [Display(Name = "Поправен")]
        public bool IsFixed { get; set; }
    }
}