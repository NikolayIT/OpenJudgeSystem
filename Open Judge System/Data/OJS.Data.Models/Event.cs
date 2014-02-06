namespace OJS.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using OJS.Data.Contracts;

    public class Event : DeletableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime StartTime { get; set; }

        /// <remarks>
        /// If EndTime is null, the event happens (and should be displayed) only on the StartTime
        /// </remarks>
        public DateTime? EndTime { get; set; }

        public string Url { get; set; }
    }
}
