namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

    using Ionic.Zip;

    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.Models;
    using OJS.Web.Common;

    public class ContestsExportController : AdministrationBaseController
    {
        public ContestsExportController(IOjsData data)
            : base(data)
        {
        }

        public FileResult Results(int id, bool compete)
        {
            var contest = this.Data.Contests.GetById(id);

            var data = new
            {
                contest.Id,
                contest.Name,
                Problems = contest.Problems.AsQueryable().OrderBy(x => x.OrderBy).ThenBy(x => x.Name),
                Questions = contest.Questions.OrderBy(x => x.Id),
                Results = this.Data.Participants.All()
                    .Where(participant => participant.ContestId == contest.Id && participant.IsOfficial == compete)
                    .Select(participant => new
                    {
                        ParticipantUserName = participant.User.UserName,
                        ParticipantFirstName = participant.User.UserSettings.FirstName,
                        ParticipantLastName = participant.User.UserSettings.LastName,
                        Answers = participant.Answers.OrderBy(answer => answer.ContestQuestionId),
                        ProblemResults = participant.Contest.Problems
                            .Select(problem =>
                                new
                                {
                                    Id = problem.Id,
                                    ProblemName = problem.Name,
                                    ProblemOrderBy = problem.OrderBy,
                                    ShowResult = problem.ShowResults,
                                    BestSubmission = problem.Submissions
                                                        .Where(z => z.ParticipantId == participant.Id)
                                                        .OrderByDescending(z => z.Points).ThenByDescending(z => z.Id)
                                                        .Select(z => new { Id = z.Id, Points = z.Points })
                                                        .FirstOrDefault()
                                })
                                .OrderBy(res => res.ProblemOrderBy).ThenBy(res => res.ProblemName),
                    })
                    .ToList()
                    .Select(x => new { Data = x, Total = x.ProblemResults.Where(y => y.ShowResult).Sum(y => y.BestSubmission == null ? 0 : y.BestSubmission.Points) })
                    .OrderByDescending(x => x.Total)
            };

            var workbook = new HSSFWorkbook();
            var sheet = workbook.CreateSheet();

            // Header
            var headerRow = sheet.CreateRow(0);
            int columnNumber = 0;
            headerRow.CreateCell(columnNumber++).SetCellValue("Username");
            headerRow.CreateCell(columnNumber++).SetCellValue("Name");
            foreach (var question in data.Questions)
            {
                headerRow.CreateCell(columnNumber++).SetCellValue(question.Text);
            }

            foreach (var problem in data.Problems)
            {
                headerRow.CreateCell(columnNumber++).SetCellValue(problem.Name);
            }

            headerRow.CreateCell(columnNumber++).SetCellValue("Total");

            // All rows
            int rowNumber = 1;
            foreach (var result in data.Results)
            {
                int cellNumber = 0;
                var row = sheet.CreateRow(rowNumber++);
                row.CreateCell(cellNumber++).SetCellValue(result.Data.ParticipantUserName);
                row.CreateCell(cellNumber++).SetCellValue(string.Format("{0} {1}", result.Data.ParticipantFirstName, result.Data.ParticipantLastName).Trim());
                foreach (var answer in result.Data.Answers)
                {
                    var answerId = 0;
                    if (answer.ContestQuestion.Type == ContestQuestionType.DropDown && int.TryParse(answer.Answer, out answerId))
                    {
                        // TODO: N+1 query problem. Optimize it.
                        var answerText =
                            this.Data.ContestQuestionAnswers.All()
                                .Where(x => x.Id == answerId)
                                .Select(x => x.Text)
                                .FirstOrDefault();
                        row.CreateCell(cellNumber++).SetCellValue(answerText);
                    }
                    else
                    {
                        row.CreateCell(cellNumber++).SetCellValue(answer.Answer);
                    }
                }

                foreach (var problemResult in result.Data.ProblemResults)
                {
                    if (problemResult.BestSubmission != null)
                    {
                        row.CreateCell(cellNumber++).SetCellValue(problemResult.BestSubmission.Points);
                    }
                    else
                    {
                        row.CreateCell(cellNumber++, CellType.Blank);
                    }
                }

                row.CreateCell(cellNumber++).SetCellValue(result.Total);
            }

            // Auto-size all columns
            for (int i = 0; i < columnNumber; i++)
            {
                sheet.AutoSizeColumn(i);
            }

            // Write the workbook to a memory stream
            var outputStream = new MemoryStream();
            workbook.Write(outputStream);

            // Return the result to the end user
            return this.File(
                outputStream.ToArray(), // The binary data of the XLS file
                "application/vnd.ms-excel", // MIME type of Excel files
                string.Format("Класиране за {0} {1}.xls", compete ? "състезание" : "практика", contest.Name)); // Suggested file name in the "Save as" dialog which will be displayed to the end user
        }

        public ActionResult Solutions(int id, bool compete, bool bestSubmissions)
        {
            var contestName = this.Data.Contests.All().Where(x => x.Id == id).Select(c => c.Name).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(contestName))
            {
                this.TempData[GlobalConstants.DangerMessage] = "Няма такова състезание";
                return this.RedirectToAction("Index", "Home", new { area = string.Empty });
            }

            var problems = this.GetContestProblems(id);

            var participants = this.GetContestParticipants(id, compete);

            var fileComment = PrepareSolutionsFileComment(compete, contestName, participants.Count, problems);

            var file = PrepareSolutionsZipFile(fileComment);

            if (bestSubmissions)
            {
                ZipParticipantsSolutions(participants, file, problems, this.GetBestSubmission);
            }
            else
            {
                ZipParticipantsSolutions(participants, file, problems, this.GetLastSubmission);
            }

            // Send file to the user
            var zipFileName = string.Format(
                "{1} {2} submissions for {0}.zip",
                contestName,
                compete ? "Contest" : "Practice",
                bestSubmissions ? "best" : "last");
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
            fileComment.AppendLine(string.Format("Number of participants: {0}", participantsCount));
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

            return fileComment;
        }

        private static void ZipParticipantsSolutions(
            IEnumerable<ParticipantModel> participants,
            ZipFile file,
            IEnumerable<ProblemModel> problems,
            Func<int, int, Submission> getSubmission)
        {
            foreach (var participant in participants)
            {
                // Create directory with the participants name
                var directoryName =
                    string.Format("{0} ({1} {2})", participant.UserName, participant.FirstName, participant.LastName).ToValidFilePath();

                file.AddDirectoryByName(directoryName);

                foreach (var problem in problems)
                {
                    var submission = getSubmission(participant.Id, problem.Id);

                    if (submission != null)
                    {
                        var fileName = 
                            string.Format("{0}.{1}", problem.Name, submission.FileExtension ?? submission.SubmissionType.FileNameExtension)
                                .ToValidFileName();

                        var content = submission.IsBinaryFile
                            ? submission.Content
                            : submission.ContentAsString.ToByteArray();

                        var entry = file.AddEntry(string.Format("{0}\\{1}", directoryName, fileName), content);
                        entry.CreationTime = submission.CreatedOn;
                        entry.ModifiedTime = submission.CreatedOn;
                    }
                }
            }
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

        private IList<ParticipantModel> GetContestParticipants(int contestId, bool isOfficialContest)
        {
            var participants = this.Data.Participants
                .All()
                .Where(x => x.ContestId == contestId && x.IsOfficial == isOfficialContest)
                .Select(ParticipantModel.Model)
                .OrderBy(x => x.UserName)
                .ToList();

            return participants;
        }

        private IList<ProblemModel> GetContestProblems(int contestId)
        {
            var problems = this.Data.Problems
                .All()
                .Where(p => p.ContestId == contestId)
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
    }
}
