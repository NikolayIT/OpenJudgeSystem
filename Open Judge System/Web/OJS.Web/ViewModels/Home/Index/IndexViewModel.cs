namespace OJS.Web.ViewModels.Home.Index
{
    using System.Collections.Generic;

    public class IndexViewModel
    {
        public IEnumerable<IndexContestViewModel> ActiveContests { get; set; }

        public IEnumerable<IndexContestViewModel> PastContests { get; set; }

        public IEnumerable<IndexContestViewModel> FutureContests { get; set; }
    }
}