namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Common.Extensions;
    using OJS.Data.Contracts;

    public class SourceCode : DeletableEntity
    {
        public int Id { get; set; }

        public string AuthorId { get; set; }

        public virtual UserProfile Author { get; set; }

        public int? ProblemId { get; set; }

        public virtual Problem Problem { get; set; }

        public byte[] Content { get; set; }

        [NotMapped]
        public string ContentAsString
        {
            get
            {
                return this.Content.Decompress();
            }

            set
            {
                this.Content = value.Compress();
            }
        }

        public bool IsPublic { get; set; }
    }
}
