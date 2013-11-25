namespace OJS.Web.Common
{
    public class SystemMessage
    {
        public string Content { get; set; }

        public int Importance { get; set; }

        public SystemMessageType Type { get; set; }
    }
}
