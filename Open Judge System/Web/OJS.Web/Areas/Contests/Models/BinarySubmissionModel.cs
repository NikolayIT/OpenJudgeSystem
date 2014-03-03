namespace OJS.Web.Areas.Contests.Models
{
    using System.ComponentModel.DataAnnotations;

    public class BinarySubmissionModel
    {
        public int ProblemId { get; set; }

        public int SubmissionTypeId { get; set; }

        [Required]
        public byte[] Content { get; set; }
    }
}