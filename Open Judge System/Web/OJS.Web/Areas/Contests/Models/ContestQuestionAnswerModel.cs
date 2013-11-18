namespace OJS.Web.Areas.Contests.Models
{
    using System.ComponentModel.DataAnnotations;

    public class ContestQuestionAnswerModel
    {
        public int QuestionId { get; set; }

        [Required]
        [StringLength(int.MaxValue, MinimumLength = 1, ErrorMessage = "Моля отговорете на въпроса.")]
        public string Answer { get; set; }
    }
}
