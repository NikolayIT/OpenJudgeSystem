namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using OJS.Common;
    using OJS.Data.Contracts;

    public class FeedbackReport : DeletableEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(GlobalConstants.FeedbackContentMinLength)]
        public string Content { get; set; }

        public virtual UserProfile User { get; set; }

        public bool IsFixed { get; set; }
    }
}
