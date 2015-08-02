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
        private ICollection<ContestCategory> children;

        private ICollection<Contest> contests; 

        public ContestCategory()
        {
            this.children = new HashSet<ContestCategory>();
            this.contests = new HashSet<Contest>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(GlobalConstants.ContestCategoryNameMaxLength)]
        [MinLength(GlobalConstants.ContestCategoryNameMinLength)]
        public string Name { get; set; }

        [DefaultValue(0)]
        public int OrderBy { get; set; }

        [ForeignKey("Parent")]
        public int? ParentId { get; set; }

        public virtual ContestCategory Parent { get; set; }

        [InverseProperty("Parent")]
        public virtual ICollection<ContestCategory> Children
        {
            get { return this.children; }
            set { this.children = value; }
        }

        public virtual ICollection<Contest> Contests
        {
            get { return this.contests; }
            set { this.contests = value; }
        }

        public bool IsVisible { get; set; }
    }
}
