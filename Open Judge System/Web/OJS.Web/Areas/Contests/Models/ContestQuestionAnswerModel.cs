using System;
namespace OJS.Web.Areas.Contests.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;

    public class ContestQuestionAnswerModel
    {
        public int QuestionId { get; set; }

        [Required]
        [StringLength(int.MaxValue, MinimumLength = 1, ErrorMessage = "Моля отговорете на въпроса.")]
        public string Answer { get; set; }
    }
}
