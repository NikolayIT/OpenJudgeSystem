namespace OJS.Web.Areas.Administration.ViewModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class FeedbackReportViewModel
    {
        public FeedbackReportViewModel()
        {
        }

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

        public int Id { get; set; }

        [Display(Name = "Име")]
        public string Name { get; set; }

        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Display(Name = "Съдържание")]
        public string Content { get; set; }

        [Display(Name = "Потребител")]
        public string Username { get; set; }

        [Display(Name = "Поправен")]
        public bool IsFixed { get; set; }

        [Display(Name = "Създаден на")]
        public DateTime? CreatedOn { get; set; }

        [Display(Name = "Променен на")]
        public DateTime? ModifiedOn { get; set; }
    }
}