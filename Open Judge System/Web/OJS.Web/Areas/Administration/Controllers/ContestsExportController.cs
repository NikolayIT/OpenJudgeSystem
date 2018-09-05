namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

    using Ionic.Zip;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.Participants;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.Models;
    using OJS.Web.Areas.Contests.Models;
    using OJS.Web.Common;
    using OJS.Web.Common.Extensions;

    public class ContestsExportController : LecturerBaseController
    {
        private readonly IContestsDataService contestsData;
        private readonly IParticipantsDataService participantsData;

        public ContestsExportController(
            IOjsData data,
            IContestsDataService contestsData,
            IParticipantsDataService participantsData)
            : base(data)
        {
            this.contestsData = contestsData;
            this.participantsData = participantsData;
        }

        public ActionResult Solutions(int id, bool compete, SubmissionExportType exportType)
        {
            var userHasAccessToContest = this.User.IsAdmin() ||
                this.contestsData.IsUserLecturerInByContestAndUser(id, this.UserProfile.Id);

            if (!userHasAccessToContest)
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var contestName = this.contestsData.GetNameById(id);
            if (string.IsNullOrWhiteSpace(contestName))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Няма такова състезание";
                return this.RedirectToAction("Index", "Home", new { area = string.Empty });
            }

            var problems = this.GetContestProblems(id);

            var participants = this.GetContestParticipants(id, compete);

            var fileComment = PrepareSolutionsFileComment(compete, contestName, participants.Count, problems);

            var file = PrepareSolutionsZipFile(fileComment);

            Func<int, int, Submission> getSubmissionFunc = null;
            switch (exportType)
            {
                case SubmissionExportType.BestSubmissions:
                    getSubmissionFunc = this.GetBestSubmission;
                    break;
                case SubmissionExportType.LastSubmissions:
                    getSubmissionFunc = this.GetLastSubmission;
                    break;
            }

            this.ZipParticipantsSolutions(participants, file, problems, getSubmissionFunc);

            // Send file to the user
            var zipFileName = string.Format(
                "{1} {2} submissions for {0}.zip",
                contestName,
                compete ? "Contest" : "Practice",
                exportType == SubmissionExportType.BestSubmissions ? "best" : exportType == SubmissionExportType.LastSubmissions ? "last" : "all");
            return new ZipFileResult(file, zipFileName);
        }

        private static StringBuilder PrepareSolutionsFileComment(
            bool isOfficialContest,
            string contestName,
            int participantsCount,
            IEnumerable<ProblemModel> problems)
        {
            var fileComment = new StringBuilder();

            fileComment.AppendLine(string.Format("{1} submissions for {0}", contestName, isOfficialContest ? "Contest" : "Practice"));
            fileComment.AppendLine($"Number of participants: {participantsCount}");
            fileComment.AppendLine();

            fileComment.AppendLine("Problems:");
            foreach (var problem in problems)
            {
                fileComment.AppendLine(string.Format(
                    "{0} - {1} points, time limit: {2:0.000} sec., memory limit: {3:0.00} MB",
                    problem.Name,
                    problem.MaximumPoints,
                    problem.TimeLimit / 1000.0,
                    problem.MemoryLimit / 1024.0 / 1024.0));
            }

            return fileComment;
        }

        private static void ZipSubmission(
            ZipFile file,
            ProblemModel problem,
            Submission submission,
            string directoryName,
            bool includeDateInFileName = false)
        {
            var fileNameWithoutExtension =
                includeDateInFileName ? $"{problem.Name}-{submission.CreatedOn.ToString("dd-MM-yyyy HH:mm:ss")}" : problem.Name;

            var fileName =
                $"{fileNameWithoutExtension}.{submission.FileExtension ?? submission.SubmissionType.FileNameExtension}"
                    .ToValidFileName();

            var content = submission.IsBinaryFile ? submission.Content : submission.ContentAsString.ToByteArray();

            var entry = file.AddEntry($"{directoryName}\\{fileName}", content);
            entry.CreationTime = submission.CreatedOn;
            entry.ModifiedTime = submission.CreatedOn;
        }

        private static ZipFile PrepareSolutionsZipFile(StringBuilder fileComment)
        {
            var file = new ZipFile
            {
                Comment = fileComment.ToString(),
                AlternateEncoding = Encoding.UTF8,
                AlternateEncodingUsage = ZipOption.AsNecessary
            };

            return file;
        }

        private void ZipParticipantsSolutions(
            IEnumerable<ParticipantModel> participants,
            ZipFile file,
            ICollection<ProblemModel> problems,
            Func<int, int, Submission> getSubmission)
        {
            foreach (var participant in participants)
            {
                // Create directory with the participants name
                var directoryName = $"{participant.UserName} ({participant.FirstName} {participant.LastName})"
                    .ToValidFilePath();
                file.AddDirectoryByName(directoryName);

                foreach (var problem in problems)
                {
                    // All submissions
                    if (getSubmission == null)
                    {
                        var submissions = this.GetAllSubmissions(participant.Id, problem.Id);

                        foreach (var submission in submissions)
                        {
                            ZipSubmission(file, problem, submission, directoryName, true);
                        }
                    }
                    else
                    {
                        var submission = getSubmission(participant.Id, problem.Id);

                        if (submission != null)
                        {
                            ZipSubmission(file, problem, submission, directoryName);
                        }
                    }
                }
            }
        }

        private IList<ParticipantModel> GetContestParticipants(int contestId, bool isOfficialContest)
        {
            var participants = this.participantsData
                .GetAllByContestAndIsOfficial(contestId, isOfficialContest)
                .Select(ParticipantModel.Model)
                .OrderBy(x => x.UserName)
                .ToList();

            return participants;
        }

        private IList<ProblemModel> GetContestProblems(int contestId)
        {
            var problems = this.Data.Problems
                .All()
                .Where(p => p.ProblemGroup.ContestId == contestId)
                .OrderBy(x => x.OrderBy)
                .ThenBy(x => x.Name)
                .Select(ProblemModel.Model)
                .ToList();

            return problems;
        }

        private Submission GetBestSubmission(int participantId, int problemId)
        {
            return this.Data.Submissions
                .All()
                .Where(submission => submission.ParticipantId == participantId && submission.ProblemId == problemId)
                .OrderByDescending(submission => submission.Points)
                .ThenByDescending(submission => submission.CreatedOn)
                .FirstOrDefault();
        }

        private Submission GetLastSubmission(int participantId, int problemId)
        {
            return this.Data.Submissions
                .All()
                .Where(submission => submission.ParticipantId == participantId && submission.ProblemId == problemId)
                .OrderByDescending(submission => submission.CreatedOn)
                .FirstOrDefault();
        }

        private IEnumerable<Submission> GetAllSubmissions(int participantId, int problemId)
        {
            return this.Data.Submissions
                .All()
                .Where(submission => submission.ParticipantId == participantId && submission.ProblemId == problemId)
                .OrderByDescending(submission => submission.CreatedOn)
                .ToList();
        }
    }
}
