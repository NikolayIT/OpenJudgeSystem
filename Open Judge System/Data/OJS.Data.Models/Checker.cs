namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using OJS.Common;
    using OJS.Data.Contracts;

    public class Checker : DeletableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MinLength(GlobalConstants.CheckerNameMinLength)]
        [MaxLength(GlobalConstants.CheckerNameMaxLength)]
        public string Name { get; set; }

        public string Description { get; set; }

        public string DllFile { get; set; }

        public string ClassName { get; set; }

        public string Parameter { get; set; }
    }
}
