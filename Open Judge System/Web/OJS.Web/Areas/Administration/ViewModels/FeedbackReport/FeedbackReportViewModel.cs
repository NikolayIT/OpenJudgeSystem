namespace OJS.Web.Areas.Administration.ViewModels.FeedbackReport
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    public class FeedbackReportViewModel : AdministrationViewModel
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
                    Username = feedback.User == null ? "Anonymous" : feedback.User.UserName,
                    IsFixed = feedback.IsFixed,
                    CreatedOn = feedback.CreatedOn,
                    ModifiedOn = feedback.ModifiedOn
                };
            }
        }

        [Display(Name = "№")]
        [DefaultValue(null)]
        [HiddenInput(DisplayValue = false)]
        public int? Id { get; set; }

        [Display(Name = "Име")]
        [Required(ErrorMessage = "Името е задължително")]
        [UIHint("SingleLineText")]
        public string Name { get; set; }

        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "Имейла е задължителен")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Невалиден имейл адрес")]
        [UIHint("SingleLineText")]
        public string Email { get; set; }

        [Display(Name = "Съдържание")]
        [Required(ErrorMessage = "Съдържанието е задължително")]
        [DataType(DataType.MultilineText)]
        [UIHint("MultiLineText")]
        public string Content { get; set; }

        [Display(Name = "Потребител")]
        [UIHint("NonEditable")]
        public string Username { get; set; }

        [Display(Name = "Поправен")]
        public bool IsFixed { get; set; }

        public FeedbackReport GetEntity(FeedbackReport report = null)
        {
            if (report == null)
            {
                report = new FeedbackReport();
            }

            report.Id = this.Id ?? default(int);
            report.Name = this.Name;
            report.Email = this.Email;
            report.Content = this.Content;
            report.IsFixed = this.IsFixed;
            report.CreatedOn = this.CreatedOn.GetValueOrDefault();
            report.ModifiedOn = this.ModifiedOn;

            return report;
        }
    }
}