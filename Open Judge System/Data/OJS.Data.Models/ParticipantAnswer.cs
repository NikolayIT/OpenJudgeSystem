namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ParticipantAnswer
    {
        [Key]
        [Column(Order = 1)]
        public int ParticipantId { get; set; }

        [Required]
        public virtual Participant Participant { get; set; }

        [Key]
        [Column(Order = 2)]
        public int ContestQuestionId { get; set; }

        [Required]
        public virtual ContestQuestion ContestQuestion { get; set; }

        public string Answer { get; set; }
    }
}
