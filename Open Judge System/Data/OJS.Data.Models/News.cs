namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    
    using OJS.Data.Contracts;

    public class News : DeletableEntity
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Source { get; set; }

        public string Content { get; set; }

        public bool IsVisible { get; set; }
    }
}
