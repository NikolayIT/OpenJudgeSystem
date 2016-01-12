namespace OJS.Workers.Common
{
    public struct CheckerDetails
    {
        public string Comment { get; set; }

        public string ExpectedOutputFragment { get; set; }

        public string UserOutputFragment { get; set; }
    }
}