namespace OJS.Web.Areas.Contests.ViewModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class QuestionViewModel
    {
        public int QuestionId { get; set; }

        [Display(Name = "Въпрос")]
        public string Question { get; set; }

        [Required]
        [StringLength(int.MaxValue, MinimumLength = 1)]
        public string Answer { get; set; }

        public static Expression<Func<ContestQuestion, QuestionViewModel>> FromQuestion
        {
            get
            {
                return question => new QuestionViewModel
                {
                    QuestionId = question.Id,
                    Question = question.Text
                };
            }
        }
    }
}