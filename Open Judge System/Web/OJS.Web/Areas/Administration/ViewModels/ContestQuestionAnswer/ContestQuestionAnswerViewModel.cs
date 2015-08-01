namespace OJS.Web.Areas.Administration.ViewModels.ContestQuestionAnswer
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

    public class ContestQuestionAnswerViewModel : AdministrationViewModel<ContestQuestionAnswer>
    {
        public static Expression<Func<ContestQuestionAnswer, ContestQuestionAnswerViewModel>> ViewModel
        {
            get
            {
                return qa => new ContestQuestionAnswerViewModel
                {
                    AnswerId = qa.Id,
                    QuestionId = qa.QuestionId,
                    QuestionText = qa.Question.Text,
                    Text = qa.Text,
                    CreatedOn = qa.CreatedOn,
                    ModifiedOn = qa.ModifiedOn
                };
            }
        }

        [DatabaseProperty(Name = "Id")]
        [DefaultValue(null)]
        [Display(Name = "№")]
        [HiddenInput(DisplayValue = false)]
        public int? AnswerId { get; set; }

        [DatabaseProperty]
        [HiddenInput(DisplayValue = false)]
        public int? QuestionId { get; set; }

        [Display(Name = "Въпрос")]
        [UIHint("NonEditable")]
        public string QuestionText { get; set; }

        [DatabaseProperty]
        [Display(Name = "Текст")]
        [Required(ErrorMessage = "Текста е задължителен!", AllowEmptyStrings = false)]
        [StringLength(
            GlobalConstants.ContestQuestionAnswerMaxLength, 
            MinimumLength = GlobalConstants.ContestQuestionAnsweronMinLength)]
        [UIHint("SingleLineText")]
        public string Text { get; set; }
    }
}