namespace OJS.Web.Areas.Api.Controllers
{
    using System.Data.Entity;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Data;
    using OJS.Web.Areas.Api.Models;

    // TODO: Introduce class ApiController
    public class ResultsController : Controller
    {
        public ResultsController(IOjsData data)
        {
            this.Data = data;
        }

        protected IOjsData Data { get; set; }

        // TODO: Extract method from these two methods since 90% of their code is the same
        public ContentResult GetPointsByAnswer(string apiKey, int? contestId, string answer)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(answer) || !contestId.HasValue)
            {
                return this.Content(string.Format("ERROR: Invalid arguments"));
            }

            var isValidApiKey = this.Data.Users
                .All()
                .Any(x => x.Id == apiKey &&
                    (x.Roles.Any(y => y.Role.Name == GlobalConstants.AdministratorRoleName) ||
                    x.LecturerInContests.Any(y => y.ContestId == contestId.Value)));
            if (!isValidApiKey)
            {
                return this.Content(string.Format("ERROR: Invalid API key"));
            }

            var participants = this.Data.Participants.All().Where(
                x => x.IsOfficial && x.ContestId == contestId.Value && x.Answers.Any(a => a.Answer == answer));

            var participant = participants.FirstOrDefault();
            if (participant == null)
            {
                return this.Content(string.Format("ERROR: No participants found"));
            }

            if (participants.Count() > 1)
            {
                return this.Content(string.Format("ERROR: More than one participants found"));
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
                return this.Content(string.Format("ERROR: Invalid arguments"));
            }

            var isValidApiKey = this.Data.Users
                .All()
                .Any(x => x.Id == apiKey &&
                    (x.Roles.Any(y => y.Role.Name == GlobalConstants.AdministratorRoleName) ||
                    x.LecturerInContests.Any(y => y.ContestId == contestId.Value)));
            if (!isValidApiKey)
            {
                return this.Content(string.Format("ERROR: Invalid API key"));
            }

            var participants = this.Data.Participants.All().Where(
                x => x.IsOfficial && x.ContestId == contestId.Value && x.User.Email == email);

            var participant = participants.FirstOrDefault();
            if (participant == null)
            {
                return this.Content(string.Format("ERROR: No participants found"));
            }

            if (participants.Count() > 1)
            {
                return this.Content(string.Format("ERROR: More than one participants found"));
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

            var isValidApiKey = this.Data.Users
                .All()
                .Any(x => x.Id == apiKey &&
                    (x.Roles.Any(y => y.Role.Name == GlobalConstants.AdministratorRoleName) ||
                    x.LecturerInContests.Any(y => y.ContestId == contestId.Value)));
            if (!isValidApiKey)
            {
                return this.Json(new ErrorMessageViewModel("Invalid API key"), JsonRequestBehavior.AllowGet);
            }

            var participants = this.Data.Participants
                .All()
                .Where(x => x.IsOfficial && x.ContestId == contestId.Value)
                .Select(participant => new
                {
                    participant.User.UserName,
                    participant.User.Email,
                    Answer = participant.Answers.Select(answer => answer.Answer).FirstOrDefault(),
                    Points = participant.Contest.Problems
                        .Select(problem => problem.Submissions
                            .Where(z => z.ParticipantId == participant.Id)
                            .OrderByDescending(z => z.Points)
                            .Select(z => z.Points)
                            .FirstOrDefault())
                        .Sum(),
                    Minutes = participant.Submissions
                        .OrderByDescending(x => x.CreatedOn)
                        .Select(x => DbFunctions.DiffMinutes(participant.Contest.StartTime, x.CreatedOn))
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.Points)
                .ThenBy(x => x.Minutes)
                .ThenBy(x => x.UserName)
                .ToList();

            return this.Json(participants, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllResultsForContestWithPoints(string apiKey, int? contestId)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || !contestId.HasValue)
            {
                return this.Json(new ErrorMessageViewModel("Invalid arguments"), JsonRequestBehavior.AllowGet);
            }

            var isValidApiKey = this.Data.Users
                .All()
                .Any(x => x.Id == apiKey &&
                    (x.Roles.Any(y => y.Role.Name == GlobalConstants.AdministratorRoleName) ||
                    x.LecturerInContests.Any(y => y.ContestId == contestId.Value)));
            if (!isValidApiKey)
            {
                return this.Json(new ErrorMessageViewModel("Invalid API key"), JsonRequestBehavior.AllowGet);
            }

            var participants = this.Data.Participants
                .All()
                .Where(x => x.IsOfficial && x.ContestId == contestId.Value)
                .Select(participant => new
                {
                    participant.User.UserName,
                    participant.User.Email,
                    Answer = participant.Answers.Select(answer => answer.Answer).FirstOrDefault(),
                    Points = participant.Contest.Problems
                        .Select(problem => problem.Submissions
                            .Where(z => z.ParticipantId == participant.Id)
                            .OrderByDescending(z => z.Points)
                            .Select(z => z.Points)
                            .FirstOrDefault())
                        .Sum(),
                    ExamTimeInMinutes = participant.Submissions
                        .Where(x => x.Problem.ContestId == contestId.Value)
                        .OrderByDescending(x => x.CreatedOn)
                        .Select(x => DbFunctions.DiffMinutes(participant.Contest.StartTime, x.CreatedOn))
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.Points)
                .ThenBy(x => x.ExamTimeInMinutes)
                .ToList();

            return this.Json(participants, JsonRequestBehavior.AllowGet);
        }
    }
}