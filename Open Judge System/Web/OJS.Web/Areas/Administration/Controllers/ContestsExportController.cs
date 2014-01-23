namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

    using Ionic.Zip;

    using OJS.Common.Extensions;
    using OJS.Data;
    using OJS.Web.Common;
    using OJS.Web.Controllers;

    public class ContestsExportController : AdministrationController
    {
        public ContestsExportController(IOjsData data)
            : base(data)
        {
        }

        public ZipFileResult Solutions(int id, bool compete)
        {
            var contest = this.Data.Contests.GetById(id);
            var problems = contest.Problems.OrderBy(x => x.Name).ToList();
            var participants =
                this.Data.Participants.All()
                    .Where(x => x.ContestId == id && x.IsOfficial == compete)
                    .Select(
                        x => new { x.Id, x.User.UserName, x.User.UserSettings.FirstName, x.User.UserSettings.LastName, })
                    .ToList()
                    .OrderBy(x => x.UserName);

            // Prepare file comment
            var fileComment = new StringBuilder();
            fileComment.AppendLine(string.Format("{1} submissions for {0}", contest.Name, compete ? "Contest" : "Practice"));
            fileComment.AppendLine(string.Format("Number of participants: {0}", participants.Count()));
            fileComment.AppendLine();
            fileComment.AppendLine("Problems:");
            foreach (var problem in problems)
            {
                fileComment.AppendLine(
                    string.Format(
                        "{0} - {1} points, time limit: {2:0.000} sec., memory limit: {3:0.00} MB",
                        problem.Name,
                        problem.MaximumPoints,
                        problem.TimeLimit / 1000.0,
                        problem.MemoryLimit / 1024.0 / 1024.0));
            }

            // Prepare zip file
            var file = new ZipFile
            {
                Comment = fileComment.ToString(),
                AlternateEncoding = Encoding.UTF8,
                AlternateEncodingUsage = ZipOption.AsNecessary
            };

            // Add participants solutions
            foreach (var participant in participants)
            {
                // Create directory with the participants name
                var directoryName =
                    string.Format("{0} ({1} {2})", participant.UserName, participant.FirstName, participant.LastName)
                        .ToValidFilePath();
                var directory = file.AddDirectoryByName(directoryName);

                foreach (var problem in problems)
                {
                    // Find submission
                    var bestSubmission =
                        this.Data.Submissions.All()
                            .Where(
                                submission =>
                                submission.ParticipantId == participant.Id && submission.ProblemId == problem.Id)
                            .OrderByDescending(submission => submission.Points)
                            .ThenByDescending(submission => submission.Id)
                            .FirstOrDefault();

                    // Create file if submission exists
                    if (bestSubmission != null)
                    {
                        var fileName =
                            string.Format("{0}.{1}", problem.Name, bestSubmission.SubmissionType.FileNameExtension)
                                .ToValidFileName();

                        var entry = file.AddEntry(
                            string.Format("{0}\\{1}", directoryName, fileName),
                            bestSubmission.ContentAsString.ToByteArray());
                        entry.CreationTime = bestSubmission.CreatedOn;
                        entry.ModifiedTime = bestSubmission.CreatedOn;
                    }
                }
            }

            // Send file to the user
            var zipFileName = string.Format("{1} submissions for {0}.zip", contest.Name, compete ? "Contest" : "Practice");
            return new ZipFileResult(file, zipFileName);
        }
    }
}
