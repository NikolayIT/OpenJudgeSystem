namespace SubmissionScoresMigrator
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    using OJS.Data;
    using OJS.Data.Models;

    public class Startup
    {
        public static void Main()
        {
            var db = GetData();

            var processed = 0;
            var page = 0;
            var pageSize = 1000;

            while (true)
            {
                Console.WriteLine("Reading data for submissions...");

                var currentSubmissions = GetSubmissions(db.Submissions, page, pageSize);
                if (!currentSubmissions.Any())
                {
                    break;
                }

                page++;

                foreach (var submission in currentSubmissions)
                {
                    submission.CacheTestRuns();

                    db.Entry(submission).State = EntityState.Modified;

                    processed++;

                    if (processed % 200 == 0)
                    {
                        db.SaveChanges();
                        db = GetData();
                    }

                    if (processed % 1000 == 0)
                    {
                        Console.WriteLine($"{processed} submissions processed");
                    }
                }
            }

            db.SaveChanges();
            db = GetData();

            Console.WriteLine($"{processed} submissions processed");

            processed = 0;
            page = 0;
            pageSize = 10000;

            while (true)
            {
                Console.WriteLine("Reading data for scores...");

                var currentSubmissions = GetBestSubmissions(db.Submissions, page, pageSize);
                if (!currentSubmissions.Any())
                {
                    break;
                }

                page++;

                foreach (var submission in currentSubmissions)
                {
                    if (submission.ProblemId.HasValue
                        && submission.ParticipantId.HasValue)
                    {
                        db.ParticipantScores.Add(new ParticipantScore
                        {
                            ProblemId = submission.ProblemId.Value,
                            ParticipantId = submission.ParticipantId.Value,
                            SubmissionId = submission.SubmissionId,
                            ParticipantName = submission.ParticipantName,
                            Points = submission.Points,
                            IsOfficial = submission.IsOfficial
                        });
                    }

                    processed++;

                    if (processed % 200 == 0)
                    {
                        db.SaveChanges();
                        db = GetData();
                    }

                    if (processed % 1000 == 0)
                    {
                        Console.WriteLine($"{processed} scores processed");
                    }
                }
            }

            db.SaveChanges();
            db = GetData();

            Console.WriteLine($"{processed} scores processed");
            Console.WriteLine("Done!");
        }

        // Public for testing
        public static SubmissionScoreModel[] GetBestSubmissions(IQueryable<Submission> submissions, int page, int pageSize)
            => submissions
                .Where(x => !x.IsDeleted)
                .GroupBy(x => new { x.ParticipantId, x.ProblemId, x.Participant.IsOfficial })
                .Select(x => x.OrderByDescending(z => z.Points).ThenByDescending(z => z.Id).FirstOrDefault())
                .OrderByDescending(x => x.Id)
                .Skip(page * pageSize)
                .Take(pageSize)
                .Select(x => new SubmissionScoreModel
                {
                    SubmissionId = x.Id,
                    ProblemId = x.ProblemId,
                    ParticipantId = x.ParticipantId,
                    ParticipantName = x.Participant.User.UserName,
                    Points = x.Points,
                    IsOfficial = x.Participant.IsOfficial
                })
                .ToArray();

        private static Submission[] GetSubmissions(IQueryable<Submission> submissions, int page, int pageSize)
            => submissions
                .Where(x => !x.IsDeleted)
                .Include(x => x.TestRuns)
                .Include(x => x.TestRuns.Select(y => y.Test))
                .OrderBy(x => x.Id)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToArray();

        private static OjsDbContext GetData()
        {
            var db = new OjsDbContext();
            db.Database.CommandTimeout = 10 * 60;

            db.DbContext.Configuration.AutoDetectChangesEnabled = false;
            db.DbContext.Configuration.ValidateOnSaveEnabled = false;
            db.DbContext.Configuration.ProxyCreationEnabled = false;
            db.DbContext.Configuration.LazyLoadingEnabled = false;

            return db;
        }
    }
}
