namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Data.Contracts;

    public class Ip : AuditInfo
    {
        private ICollection<ContestIp> contests;

        public Ip()
        {
            this.contests = new HashSet<ContestIp>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [Index(IsUnique = true)]
        [MaxLength(45)]
        public string Value { get; set; }

        public string Description { get; set; }

        public virtual ICollection<ContestIp> Contests
        {
            get { return this.contests; }
            set { this.contests = value; }
        }
    }
}
