namespace OJS.Web.Areas.Users.ViewModels
{
    public class UserParticipationViewModel
    {
        public int ContestId { get; set; }

        public string ContestName { get; set; }

        public int ContestResult { get; set; }

        public bool IsOfficial { get; set; }
    }
}