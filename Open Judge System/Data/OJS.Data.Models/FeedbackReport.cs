namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using OJS.Data.Contracts;

    public class FeedbackReport : DeletableEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string Content { get; set; }

        public string UserId { get; set; }

        public virtual UserProfile User { get; set; }

        public bool IsFixed { get; set; }
    }
}
