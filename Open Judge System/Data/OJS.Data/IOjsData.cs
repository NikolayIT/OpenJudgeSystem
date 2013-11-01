namespace OJS.Data
{
    using System;

    using OJS.Data.Contracts;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public interface IOjsData : IDisposable
    {
        IContestsRepository Contests { get; }

        IRepository<Problem> Problems { get; }

        IRepository<Test> Tests { get; }

        IRepository<News> News { get; }

        IRepository<ContestCategory> ContestCategories { get; }

        IRepository<TestRun> TestRuns { get; }

        IRepository<FeedbackReport> FeedbackReports { get; }

        IRepository<Checker> Checkers { get; }

        IRepository<ProblemResource> Resources { get; }

        IUsersRepository Users { get; }

        IParticipantsRepository Participants { get; }

        IRepository<Setting> Settings { get; }

        ISubmissionsRepository Submissions { get; }

        IOjsDbContext Context { get; }

        int SaveChanges();
    }
}
