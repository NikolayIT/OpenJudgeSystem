namespace OJS.Web.ViewModels.News
{
    using System.Collections.Generic;

    public class AllNewsViewModel
    {
        public IEnumerable<NewsViewModel> AllNews { get; set; }

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int AllPages { get; set; }
    }
}