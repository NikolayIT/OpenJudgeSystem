namespace OJS.Web.Areas.Administration.ViewModels.ContestQuestionAnswer
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;

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
                    Text = qa.Text
                };
            }
        }

        [Display(Name = "№")]
        [DefaultValue(null)]
        [HiddenInput(DisplayValue = false)]
        public int? AnswerId { get; set; }

        [DatabaseProperty]
        [HiddenInput(DisplayValue = false)]
        public int? QuestionId { get; set; }

        [Display(Name = "Въпрос")]
        public  string QuestionText { get; private set; }

        [DatabaseProperty]
        [Display(Name = "Текст")]
        [Required(ErrorMessage = "Текста е задължителен!", AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 5)]
        [UIHint("SingleLineText")]
        public string Text { get; set; }

        public override ContestQuestionAnswer GetEntityModel(ContestQuestionAnswer model = null)
        {
            model = model ?? new ContestQuestionAnswer();
            model.Id = this.AnswerId.GetValueOrDefault();
            return this.ConvertToDatabaseEntity(model);
        }
    }
}