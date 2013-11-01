namespace OJS.Data.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    using OJS.Data.Contracts;

    public class News : DeletableEntity
    {
        [Key]
        [Display(Name = "№")]
        public int Id { get; set; }

        [Display(Name = "Заглавие")]
        [Required(ErrorMessage = "Заглавието е задължително!")]
        public string Title { get; set; }

        [Display(Name = "Автор")]
        [Required(ErrorMessage = "Автора е задължителен!")]
        public string Author { get; set; }

        [Display(Name = "Източник")]
        public string Source { get; set; }

        [Display(Name = "Съдържание")]
        [Required(ErrorMessage = "Съдържанието е задължително!")]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }

        [Display(Name = "Видимост")]
        public bool IsVisible { get; set; }
    }
}
