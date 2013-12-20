namespace OJS.Web.Areas.Contests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Data.Models;

    using Resource = Resources.Areas.Contests.ViewModels.ContestsViewModels;

    public class QuestionViewModel
    {
        public static Expression<Func<ContestQuestion, QuestionViewModel>> FromQuestion
        {
            get
            {
                return question => new QuestionViewModel
                {
                    QuestionId = question.Id,
                    Question = question.Text,
                    Type = question.Type,
                    RegularExpression = question.RegularExpressionValidation,
                    PossibleAnswers = question.Answers.Select(x => new DropDownAnswerViewModel
                    {
                        Text = x.Text,
                        Value = x.Id
                    })
                };
            }
        }

        public int QuestionId { get; set; }

        public string RegularExpression { get; set; }

        [Display(Name = "Question", ResourceType = typeof(Resource))]
        public string Question { get; set; }

        [Required]
        public string Answer { get; set; }

        public ContestQuestionType Type { get; set; }

        public IEnumerable<DropDownAnswerViewModel> PossibleAnswers { get; set; }

        public IEnumerable<SelectListItem> DropDownItems
        {
            get
            {
                return this.PossibleAnswers.Select(x => new SelectListItem
                {
                    Text = x.Text,
                    Value = x.Value.ToString()
                });
            }
        }
    }
}