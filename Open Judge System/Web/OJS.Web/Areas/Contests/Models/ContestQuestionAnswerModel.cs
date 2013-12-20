namespace OJS.Web.Areas.Contests.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public class ContestQuestionAnswerModel
    {
        public int QuestionId { get; set; }

        [Required]
        public string Answer { get; set; }
    }
}
