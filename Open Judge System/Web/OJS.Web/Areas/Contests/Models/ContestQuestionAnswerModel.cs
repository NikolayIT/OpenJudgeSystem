namespace OJS.Web.Areas.Contests.Models
{
    using System.ComponentModel.DataAnnotations;

    public class ContestQuestionAnswerModel
    {
        public int QuestionId { get; set; }

        [Required]
        public string Answer { get; set; }
    }
}
