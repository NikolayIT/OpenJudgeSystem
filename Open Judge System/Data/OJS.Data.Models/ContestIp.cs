namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ContestIp
    {
        [Key]
        [Column(Order = 1)]
        public int ContestId { get; set; }

        public virtual Contest Contest { get; set; }

        [Key]
        [Column(Order = 2)]
        public int IpId { get; set; }

        public virtual Ip Ip { get; set; }

        public bool IsOriginallyAllowed { get; set; }
    }
}
