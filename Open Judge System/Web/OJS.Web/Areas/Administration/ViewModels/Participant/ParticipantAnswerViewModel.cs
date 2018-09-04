namespace OJS.Web.Areas.Administration.ViewModels.Participant
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using OJS.Data.Models;

    using static OJS.Common.Constants.EditorTemplateConstants;

    using Resource = Resources.Areas.Administration.Participants.ViewModels.ParticipantViewModels;

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

        [Display(Name = nameof(Resource.Question), ResourceType = typeof(Resource))]
        [UIHint(NonEditable)]
        public string QuestionText { get; set; }

        [Display(Name = nameof(Resource.Answer), ResourceType = typeof(Resource))]
        [Required(
            ErrorMessageResourceName = nameof(Resource.Answer_required),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint(SingleLineText)]
        public string Answer { get; set; }
    }
}