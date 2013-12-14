namespace OJS.Web.Areas.Contests.ViewModels
{
    using System;
    using System.Linq.Expressions;

    using OJS.Data.Models;
    using System.ComponentModel.DataAnnotations;

    public class SubmissionResultIsCompiledViewModel
    {
        public static Expression<Func<Submission, SubmissionResultIsCompiledViewModel>> FromSubmission
        {
            get
            {
                return s => new SubmissionResultIsCompiledViewModel
                {
                    Id = s.Id,
                    IsCompiledSuccessfully = s.IsCompiledSuccessfully,
                    SubmissionDate = s.CreatedOn,
                    IsCalculated = s.Processed
                };
            }
        }

        public int Id { get; set; }

        [Display(Name = "Компилация")]
        public bool IsCompiledSuccessfully { get; set; }

        [Display(Name = "Изпратено на")]
        public DateTime SubmissionDate { get; set; }

        public bool IsCalculated { get; set; }
    }
}