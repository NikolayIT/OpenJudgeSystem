namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Common;
    using OJS.Data.Contracts;

    public class ContestCategory : DeletableEntity, IOrderable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(GlobalConstants.ContestCategoryNameMaxLength)]
        [MinLength(GlobalConstants.ContestCategoryNameMinLength)]
        public string Name { get; set; }

        [DefaultValue(0)]
        public int OrderBy { get; set; }

        public int? ParentId { get; set; }

        public virtual ContestCategory Parent { get; set; }

        [InverseProperty(nameof(Parent))]
        public virtual ICollection<ContestCategory> Children { get; set; } = new HashSet<ContestCategory>();

        public virtual ICollection<Contest> Contests { get; set; } = new HashSet<Contest>();

        public virtual ICollection<LecturerInContestCategory> Lecturers { get; set; } = new HashSet<LecturerInContestCategory>();

        public bool IsVisible { get; set; }
    }
}
