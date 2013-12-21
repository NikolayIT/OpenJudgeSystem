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

        public ContestQuestion ToEntity
        {
            get
            {
                return new ContestQuestion
                {
                    Id = this.Id ?? default(int),
                    ContestId = this.ContestId ?? default(int),
                    Text = this.Text,
                    AskOfficialParticipants = this.AskOfficialParticipants,
                    AskPracticeParticipants = this.AskPracticeParticipats,
                    Type = this.Type,
                    RegularExpressionValidation = this.RegularExpressionValidation,
                    CreatedOn = this.CreatedOn.GetValueOrDefault(),
                    ModifiedOn = this.ModifiedOn,
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
    }
}