﻿namespace OJS.Data
{
    using System;

    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Data.Contracts;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public interface IOjsData : IDisposable
    {
        IContestsRepository Contests { get; }

        IProblemsRepository Problems { get; }

        ITestRepository Tests { get; }

        IDeletableEntityRepository<News> News { get; }

        IDeletableEntityRepository<Event> Events { get; }

        IDeletableEntityRepository<ContestCategory> ContestCategories { get; }

        IDeletableEntityRepository<ContestQuestion> ContestQuestions { get; }

        IDeletableEntityRepository<ContestQuestionAnswer> ContestQuestionAnswers { get; }

        ITestRunsRepository TestRuns { get; }

        IDeletableEntityRepository<FeedbackReport> FeedbackReports { get; }

        IDeletableEntityRepository<Checker> Checkers { get; }

        IDeletableEntityRepository<ProblemResource> Resources { get; }

        IUsersRepository Users { get; }

        IRepository<IdentityRole> Roles { get; }

        IParticipantsRepository Participants { get; }

        IRepository<Setting> Settings { get; }

        ISubmissionsRepository Submissions { get; }

        IRepository<SubmissionType> SubmissionTypes { get; }

        IDeletableEntityRepository<SourceCode> SourceCodes { get; }

        IRepository<AccessLog> AccessLogs { get; }

        IOjsDbContext Context { get; }

        int SaveChanges();
    }
}
