namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using OJS.Common.Models;
    using OJS.Data.Contracts;

    public class ProblemGroup : DeletableEntity, IOrderable
    {
        [Key]
        public int Id { get; set; }

        public int ContestId { get; set; }

        public virtual Contest Contest { get; set; }

        public int OrderBy { get; set; }

        public ProblemGroupType? Type { get; set; }

        public virtual ICollection<Problem> Problems { get; set; } = new HashSet<Problem>();
    }
}