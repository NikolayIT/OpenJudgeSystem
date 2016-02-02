namespace OJS.Web.ViewModels.Submission
{
    public class SubmissionsFilterViewModel
    {
        public string UserId { get; set; }

        public int? ContestId { get; set; }

        public bool NotProcessedOnly { get; set; }
    }
}