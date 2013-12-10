namespace OJS.Data
{
    using System;

    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Data.Contracts;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public interface IOjsData : IDisposable
    {
        IContestsRepository Contests { get; }

        IRepository<Problem> Problems { get; }

        IRepository<Test> Tests { get; }

        IRepository<News> News { get; }

        IRepository<Event> Events { get; }

        IRepository<ContestCategory> ContestCategories { get; }

        ITestRunsRepository TestRuns { get; }

        IRepository<FeedbackReport> FeedbackReports { get; }

        IRepository<Checker> Checkers { get; }

        IRepository<ProblemResource> Resources { get; }

        IUsersRepository Users { get; }

        IRepository<IdentityRole> Roles { get; }

        IParticipantsRepository Participants { get; }

        IRepository<Setting> Settings { get; }

        ISubmissionsRepository Submissions { get; }

        IRepository<SubmissionType> SubmissionTypes { get; }

        IOjsDbContext Context { get; }

        int SaveChanges();
    }
}
