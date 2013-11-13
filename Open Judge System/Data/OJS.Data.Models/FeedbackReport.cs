namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using OJS.Data.Contracts;

    public class FeedbackReport : DeletableEntity
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Име")]
        [Required(ErrorMessage = "Името е задължително")]
        public string Name { get; set; }

        [Display(Name = "E-mail")]
        [Required(ErrorMessage = "Имейла е задължителен")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Невалиден имейл адрес")]
        public string Email { get; set; }

        [Display(Name = "Съдържание")]
        [Required(ErrorMessage = "Съдържанието е задължително")]
        [DataType(DataType.MultilineText)]
        public string Content { get; set; }

        public virtual UserProfile User { get; set; }

        public bool IsFixed { get; set; }
    }
}
