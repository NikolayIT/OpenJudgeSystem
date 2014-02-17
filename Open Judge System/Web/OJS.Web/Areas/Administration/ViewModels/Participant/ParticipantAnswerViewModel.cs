namespace OJS.Web.Areas.Administration.ViewModels.Participant
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

    using OJS.Data.Models;

    public class ParticipantAnswerViewModel
    {
        public static Expression<Func<ParticipantAnswer, ParticipantAnswerViewModel>> ViewModel
        {
            get
            {
                return pa => new ParticipantAnswerViewModel
                {
                    ParticipantId = pa.ParticipantId,
                    ContestQuestionId = pa.ContestQuestionId,
                    QuestionText = pa.ContestQuestion.Text,
                    Answer = pa.Answer
                };
            }
        }

        [HiddenInput(DisplayValue = false)]
        public int ParticipantId { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int ContestQuestionId { get; set; }

        [Display(Name = "Въпрос")]
        [UIHint("NonEditable")]
        public string QuestionText { get; set; }

        [Display(Name = "Отговор")]
        [Required(ErrorMessage = "Отговорът е задължителен!")]
        [UIHint("SingleLineText")]
        public string Answer { get; set; }
    }
}