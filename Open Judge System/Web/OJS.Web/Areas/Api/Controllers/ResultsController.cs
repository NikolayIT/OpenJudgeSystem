namespace OJS.Web.Areas.Api.Controllers
{
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Data;
    using OJS.Web.Areas.Api.Models;

    public class ResultsController : ApiController
    {
        private readonly IOjsData data;

        public ResultsController(IOjsData data)
        {
            this.data = data;
        }

        // TODO: Extract method from these two methods since 90% of their code is the same
        public ContentResult GetPointsByAnswer(string apiKey, int? contestId, string answer)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(answer) || !contestId.HasValue)
            {
                return this.Content("ERROR: Invalid arguments");
            }

            var user = this.data.Users.GetById(apiKey);
            if (user == null || user.Roles.All(x => x.Role.Name != GlobalConstants.AdministratorRoleName))
            {
                return this.Content("ERROR: Invalid API key");
            }

            var participants = this.data.Participants.All().Where(
                x => x.IsOfficial && x.ContestId == contestId.Value && x.Answers.Any(a => a.Answer == answer));

            var participant = participants.FirstOrDefault();
            if (participant == null)
            {
                return this.Content("ERROR: No participants found");
            }

            if (participants.Count() > 1)
            {
                return this.Content("ERROR: More than one participants found");
            }

            var points =
                participant.Contest.Problems.Select(
                    problem =>
                    problem.Submissions.Where(z => z.ParticipantId == participant.Id)
                        .OrderByDescending(z => z.Points)
                        .Select(z => z.Points)
                        .FirstOrDefault()).Sum();

            return this.Content(points.ToString(CultureInfo.InvariantCulture));
        }

        public ContentResult GetPointsByEmail(string apiKey, int? contestId, string email)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(email) || !contestId.HasValue)
            {
                return this.Content("ERROR: Invalid arguments");
            }

            var user = this.data.Users.GetById(apiKey);
            if (user == null || user.Roles.All(x => x.Role.Name != GlobalConstants.AdministratorRoleName))
            {
                return this.Content("ERROR: Invalid API key");
            }

            var participants = this.data.Participants.All().Where(
                x => x.IsOfficial && x.ContestId == contestId.Value && x.User.Email == email);

            var participant = participants.FirstOrDefault();
            if (participant == null)
            {
                return this.Content("ERROR: No participants found");
            }

            if (participants.Count() > 1)
            {
                return this.Content("ERROR: More than one participants found");
            }

            var points =
                participant.Contest.Problems.Select(
                    problem =>
                    problem.Submissions.Where(z => z.ParticipantId == participant.Id)
                        .OrderByDescending(z => z.Points)
                        .Select(z => z.Points)
                        .FirstOrDefault()).Sum();

            return this.Content(points.ToString(CultureInfo.InvariantCulture));
        }

        public JsonResult GetAllResultsForContest(string apiKey, int? contestId)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || !contestId.HasValue)
            {
                return this.Json(new ErrorMessageViewModel("Invalid arguments"), JsonRequestBehavior.AllowGet);
            }

            var user = this.data.Users.GetById(apiKey);
            if (user == null || user.Roles.All(x => x.Role.Name != GlobalConstants.AdministratorRoleName))
            {
                return this.Json(new ErrorMessageViewModel("Invalid API key"), JsonRequestBehavior.AllowGet);
            }

            var participants =
                this.data.Participants.All()
                    .Where(x => x.IsOfficial && x.ContestId == contestId.Value)
                    .Select(
                        participant =>
                        new
                            {
                                participant.User.UserName,
                                participant.User.Email,
                                Answer = participant.Answers.Select(answer => answer.Answer).FirstOrDefault(),
                                Points = participant.Contest.Problems.Select(
                                    problem =>
                                    problem.Submissions.Where(z => z.ParticipantId == participant.Id)
                                        .OrderByDescending(z => z.Points)
                                        .Select(z => z.Points)
                                        .FirstOrDefault()).Sum()
                            })
                    .OrderByDescending(x => x.Points)
                    .ToList();

            return this.Json(participants, JsonRequestBehavior.AllowGet);
        }
    }
}
