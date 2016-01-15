namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Data.Contracts;

    public class LecturerInContestCategory : AuditInfo
    {
        [Key]
        [Column(Order = 1)]
        public string LecturerId { get; set; }

        public virtual UserProfile Lecturer { get; set; }

        [Key]
        [Column(Order = 2)]
        public int ContestCategoryId { get; set; }

        public virtual ContestCategory ContestCategory { get; set; }
    }
}
