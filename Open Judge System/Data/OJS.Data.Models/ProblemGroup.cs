namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using OJS.Data.Contracts;

    public class ProblemGroup : IOrderable
    {
        [Key]
        public int Id { get; set; }

        public int ContestId { get; set; }

        public virtual Contest Contest { get; set; }

        public int OrderBy { get; set; }

        public ICollection<Problem> Problems { get; set; } = new HashSet<Problem>();
    }
}