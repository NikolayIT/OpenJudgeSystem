namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using OJS.Data.Contracts;

    public class Ip : AuditInfo
    {
        private ICollection<Contest> contests;

        public Ip()
        {
            this.contests = new HashSet<Contest>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Value { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Contest> Contests
        {
            get { return this.contests; }
            set { this.contests = value; }
        }
    }
}
