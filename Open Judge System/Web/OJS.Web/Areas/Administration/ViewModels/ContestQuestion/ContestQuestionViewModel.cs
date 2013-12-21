namespace OJS.Web.Areas.Administration.ViewModels.ContestQuestion
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;

    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Common;

    public class ContestQuestionViewModel : AdministrationViewModel
    {
        public static Expression<Func<ContestQuestion, ContestQuestionViewModel>> ViewModel
        {
            get
            {
                return question => new ContestQuestionViewModel
                {
                    Id = question.Id,
                    ContestId = question.ContestId,
                    Text = question.Text,
                    AskOfficialParticipants = question.AskOfficialParticipants,
                    AskPracticeParticipats = question.AskPracticeParticipants,
                    Type = question.Type,
                    RegularExpressionValidation = question.RegularExpressionValidation,
                    CreatedOn = question.CreatedOn,
                    ModifiedOn = question.ModifiedOn,
                };
            }
        }

        public int? Id { get; set; }

        public int? ContestId { get; set; }

        [Display(Name = "Текст")]
        [Required(ErrorMessage = "Текста е задължителен!", AllowEmptyStrings = false)]
        [StringLength(100, MinimumLength = 5)]
        public string Text { get; set; }

        [Display(Name = "Задаване към състезанията")]
        [DefaultValue(true)]
        public bool AskOfficialParticipants { get; set; }

        [Display(Name = "Задаване към упражненията")]
        [DefaultValue(true)]
        public bool AskPracticeParticipats { get; set; }

        [Display(Name = "Тип въпрос")]
        public ContestQuestionType Type { get; set; }

        [Display(Name = "Reg-Ex валидация")]
        public string RegularExpressionValidation { get; set; }

        public ContestQuestion ToEntity(ContestQuestion question = null)
        {
            if (question == null)
            {
                question = new ContestQuestion();
            }

            question.Id = this.Id ?? default(int);
            question.ContestId = this.ContestId ?? default(int);
            question.Text = this.Text;
            question.AskOfficialParticipants = this.AskOfficialParticipants;
            question.AskPracticeParticipants = this.AskPracticeParticipats;
            question.Type = this.Type;
            question.RegularExpressionValidation = this.RegularExpressionValidation;
            question.CreatedOn = this.CreatedOn.GetValueOrDefault();
            question.ModifiedOn = this.ModifiedOn;

            return question;
        }
    }
}