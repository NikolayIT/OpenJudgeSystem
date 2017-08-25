namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SubmissionsForProcessing")]
    public class SubmissionsForProcessing
    {
        public int Id { get; set; }

        public int SubmissionId { get; set; }

        public bool Processing { get; set; }

        public bool Processed { get; set; }
    }
}
