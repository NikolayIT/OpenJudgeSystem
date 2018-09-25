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

    using static OJS.Common.Constants.EditorTemplateConstants;

    using Resource = Resources.Areas.Administration.Contests.ViewModels.ContestQuestionAnswer;

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

        [Display(Name = nameof(Resource.Question), ResourceType = typeof(Resource))]
        [UIHint(NonEditable)]
        public string QuestionText { get; set; }

        [DatabaseProperty]
        [Display(Name = nameof(Resource.Text), ResourceType = typeof(Resource))]
        [Required(
            AllowEmptyStrings = false,
            ErrorMessageResourceName = nameof(Resource.Text_required),
            ErrorMessageResourceType = typeof(Resource))]
        [StringLength(
            GlobalConstants.ContestQuestionAnswerMaxLength,
            MinimumLength = GlobalConstants.ContestQuestionAnswerMinLength,
            ErrorMessageResourceName = nameof(Resource.Text_length),
            ErrorMessageResourceType = typeof(Resource))]
        [UIHint(SingleLineText)]
        public string Text { get; set; }
    }
}