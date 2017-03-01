namespace OJS.Web.ViewModels.Shared
{
    public class SimplePaginationViewModel
    {
        public int CurrentPage { get; set; }

        public bool HasNext { get; set; }

        public string Url { get; set; }
    }
}
