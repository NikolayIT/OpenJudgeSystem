namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using OJS.Common;
    using OJS.Data.Contracts;

    public class News : DeletableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MinLength(GlobalConstants.NewsTitleMinLength)]
        [MaxLength(GlobalConstants.NewsTitleMaxLength)]
        public string Title { get; set; }

        [Required]
        [MinLength(GlobalConstants.NewsAuthorNameMinLength)]
        [MaxLength(GlobalConstants.NewsAuthorNameMaxLength)]
        public string Author { get; set; }

        [Required]
        [MinLength(GlobalConstants.NewsSourceMinLength)]
        [MaxLength(GlobalConstants.NewsSourceMaxLength)]
        public string Source { get; set; }

        [Required]
        [MinLength(GlobalConstants.NewsContentMinLength)]
        public string Content { get; set; }

        public bool IsVisible { get; set; }
    }
}
