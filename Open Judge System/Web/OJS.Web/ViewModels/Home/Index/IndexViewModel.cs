namespace OJS.Web.ViewModels.Home.Index
{
    using System.Collections.Generic;

    public class IndexViewModel
    {
        public IEnumerable<HomeContestViewModel> ActiveContests { get; set; }

        public IEnumerable<HomeContestViewModel> PastContests { get; set; }
    }
}