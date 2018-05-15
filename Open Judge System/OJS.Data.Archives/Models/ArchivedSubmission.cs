namespace OJS.Data.Archives.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Common;

    [Table(nameof(IArchivesDbContext.Submissions))]
    public class ArchivedSubmission
    {
        [Key]
        public int Id { get; set; }

        public int? ParticipantId { get; set; }

        public int? ProblemId { get; set; }

        public int? SubmissionTypeId { get; set; }

        public byte[] Content { get; set; }

        public string FileExtension { get; set; }

        public byte[] SolutionSkeleton { get; set; }

        [StringLength(GlobalConstants.IpAdressMaxLength)]
        [Column(TypeName = "varchar")]
        public string IpAddress { get; set; }

        public bool IsCompiledSuccessfully { get; set; }

        public string CompilerComment { get; set; }

        public bool? IsPublic { get; set; }

        public string TestRunsCache { get; set; }

        public bool Processed { get; set; }

        public bool Processing { get; set; }

        public string ProcessingComment { get; set; }

        public int Points { get; set; }
    }
}