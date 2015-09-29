namespace OJS.Web.Areas.Contests.Helpers
{
    using System.Linq;

    using OJS.Data.Models;

    public static class ContestExtensions
    {
        public static bool ShouldShowRegistrationForm(this Contest contest, bool isOfficialParticipant)
        {
            // Show registration form if contest password is required
            var showRegistrationForm = (isOfficialParticipant && contest.HasContestPassword) || (!isOfficialParticipant && contest.HasPracticePassword);

            // Show registration form if contest is official and questions should be asked
            if (isOfficialParticipant && contest.Questions.Any(x => x.AskOfficialParticipants))
            {
                showRegistrationForm = true;
            }

            // Show registration form if contest is not official and questions should be asked
            if (!isOfficialParticipant && contest.Questions.Any(x => x.AskPracticeParticipants))
            {
                showRegistrationForm = true;
            }

            return showRegistrationForm;
        }
    }
}