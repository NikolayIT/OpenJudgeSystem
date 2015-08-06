namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using OJS.Data.Contracts;

    public class AccessLog : AuditInfo
    {
        [Key]
        public long Id { get; set; }

        public string UserId { get; set; }

        public virtual UserProfile User { get; set; }

        public string IpAddress { get; set; }

        public string RequestType { get; set; }

        public string Url { get; set; }

        public string PostParams { get; set; }
    }
}
