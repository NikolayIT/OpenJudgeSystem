namespace OJS.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Common;
    using OJS.Data.Contracts;

    public class ExamGroup : DeletableEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        [MaxLength(GlobalConstants.TrainingLabNameMaxLength)]
        [MinLength(GlobalConstants.TrainingLabNameMinLength)]
        public string TrainingLabNameBg { get; set; }

        [Required]
        [MaxLength(GlobalConstants.TrainingLabNameMaxLength)]
        [MinLength(GlobalConstants.TrainingLabNameMinLength)]
        public string TrainingLabNameEn { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndTate { get; set; }

        public int? ContestId { get; set; }

        public virtual Contest Contest { get; set; }

        public ICollection<UserProfile> Users { get; set; } = new HashSet<UserProfile>();
    }
}