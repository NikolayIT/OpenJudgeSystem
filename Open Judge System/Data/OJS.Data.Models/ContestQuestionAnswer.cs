namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using OJS.Common;
    using OJS.Data.Contracts;

    public class ContestQuestionAnswer : DeletableEntity
    {
        [Key]
        public int Id { get; set; }

        public int QuestionId { get; set; }

        public virtual ContestQuestion Question { get; set; }

        [MaxLength(GlobalConstants.ContestQuestionAnswerMaxLength)]
        [MinLength(GlobalConstants.ContestQuestionAnswerMinLength)]
        public string Text { get; set; }
    }
}
