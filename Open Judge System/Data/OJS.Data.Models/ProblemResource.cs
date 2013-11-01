namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using OJS.Data.Contracts;

    public class ProblemResource : DeletableEntity, IOrderable
    {
        public int Id { get; set; }

        public int ProblemId { get; set; }

        public Problem Problem { get; set; }

        public string Name { get; set; }

        public ProblemResourceType Type { get; set; }

        public byte[] File { get; set; }

        [MaxLength(4)]
        public string FileExtension { get; set; }

        public string Link { get; set; }

        public int OrderBy { get; set; }
    }
}
