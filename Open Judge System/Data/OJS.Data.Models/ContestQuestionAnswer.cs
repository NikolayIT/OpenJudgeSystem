namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    public class ContestQuestionAnswer
    {
        [Key]
        public int Id { get; set; }
        
        public int QuestionId { get; set; }

        public virtual ContestQuestion Question { get; set; }

        public string Text { get; set; }
    }
}
