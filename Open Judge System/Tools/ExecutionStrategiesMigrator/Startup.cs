namespace ExecutionStrategiesMigrator
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
            var pageSize = 500;

            while (true)
            {
                Console.WriteLine("Reading data for contests...");

                var allProblems = GetAllProblems(db.Problems, page, pageSize);
                if (!allProblems.Any())
                {
                    break;
                }

                page++;

                foreach (var problem in allProblems)
                {
                    problem.SubmissionTypes = problem.Contest.SubmissionTypes;
                    Console.WriteLine($"Problem with id = {problem.Id} modified");
                    processed++;

                    if (processed % 500 == 0)
                    {
                        db.SaveChanges();
                        db = GetData();
                    }

                    if (processed % 500 == 0)
                    {
                        Console.WriteLine($"{processed} problems processed");
                    }

                }
            }

            db.SaveChanges();
            db = GetData();

            Console.WriteLine("All submissions processed");

            processed = 0;
            page = 0;
            pageSize = 500;

            while (true)
            {
                Console.WriteLine("Reading data for submission types...");

                var problems = GetAllProblems(db.Problems, page, pageSize);
                if (!problems.Any())
                {
                    break;
                }

                page++;

                foreach (var problem in problems)
                {
                    if (problem.Contest.SubmissionTypes.Select(s => s.Id)
                        .Except(problem.SubmissionTypes.Select(x => x.Id))
                        .Any())
                    {
                        Console.WriteLine(
                            $"Submission types for problem with id = {problem.Id} were not copied correctly");
                    }
                }
            }

            db.SaveChanges();
            Console.WriteLine("Done!");
        }
        private static Problem[] GetAllProblems(IQueryable<Problem> problems, int page, int pageSize)
            => problems
                .Where(x => !x.IsDeleted)
                .Include(x => x.Contest.SubmissionTypes)
                .Include(x => x.SubmissionTypes)
                .OrderBy(x => x.Id)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToArray();

        private static OjsDbContext GetData()
        {
            var db = new OjsDbContext();
            db.Database.CommandTimeout = 10 * 60;
            return db;
        }
    }
}
