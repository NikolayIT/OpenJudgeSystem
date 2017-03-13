namespace OJS.Web.ViewModels.Shared
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }

        public int? AllPages { get; set; }

        public string Url { get; set; }
    }
}