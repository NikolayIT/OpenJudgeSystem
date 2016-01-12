namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using OJS.Common;
    using OJS.Common.Models;
    using OJS.Data.Contracts;

    public class ProblemResource : DeletableEntity, IOrderable
    {
        [Key]
        public int Id { get; set; }

        public int ProblemId { get; set; }

        public Problem Problem { get; set; }

        [Required]
        [MinLength(GlobalConstants.ProblemResourceNameMinLength)]
        [MaxLength(GlobalConstants.ProblemResourceNameMaxLength)]
        public string Name { get; set; }

        public ProblemResourceType Type { get; set; }

        public byte[] File { get; set; }

        [MaxLength(GlobalConstants.FileExtentionMaxLength)]
        public string FileExtension { get; set; }

        public string Link { get; set; }

        public int OrderBy { get; set; }
    }
}
