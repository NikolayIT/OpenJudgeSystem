namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System.Collections.Generic;
    
    using OJS.Web.Areas.Contests.ViewModels.Contests;

    public class ContestResultsViewModel
    {
        public int Id { get; set; }
        
        public string Name { get; set; }

        public int CurrentPage { get; set; }

        public int AllPages { get; set; }

        public int TotalResults { get; set; }

        public IEnumerable<ContestProblemViewModel> Problems { get; set; }

        public IEnumerable<ParticipantResultViewModel> Results { get; set; }

        public bool ContestCanBeCompeted { get; set; }

        public bool ContestCanBePracticed { get; set; }
    }
}