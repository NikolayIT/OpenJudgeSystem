namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Data.Contracts;

    public class LecturerInContest : AuditInfo
    {
        [Key]
        [Column(Order = 1)]
        public string LecturerId { get; set; }

        public virtual UserProfile Lecturer { get; set; }

        [Key]
        [Column(Order = 2)]
        public int ContestId { get; set; }

        public virtual Contest Contest { get; set; }
    }
}
