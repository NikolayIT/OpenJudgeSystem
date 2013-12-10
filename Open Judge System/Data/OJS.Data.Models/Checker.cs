namespace OJS.Data.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using OJS.Data.Contracts;

    public class Checker : DeletableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public string DllFile { get; set; }

        public string ClassName { get; set; }

        public string Parameter { get; set; }

        /* TODO:
        [ForeignKey("Problem")]
        public int? ProblemId { get; set; }

        /// <summary>
        /// The problem for which this checker is created.
        /// If null, the checker is available for all problems.
        /// </summary>
        public virtual Problem Problem { get; set; }
         */
    }
}
