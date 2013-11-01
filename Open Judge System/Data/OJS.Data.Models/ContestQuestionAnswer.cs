namespace OJS.Data.Models
{
    public class ContestQuestionAnswer
    {
        public int Id { get; set; }
        
        public int QuestionId { get; set; }

        public virtual ContestQuestion Question { get; set; }

        public string Text { get; set; }
    }
}
