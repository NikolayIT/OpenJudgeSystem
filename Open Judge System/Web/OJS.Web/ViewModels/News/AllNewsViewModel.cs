using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OJS.Web.ViewModels.News
{
    public class AllNewsViewModel
    {
        public IEnumerable<NewsViewModel> AllNews { get; set; }

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int AllPages { get; set; }
    }
}