namespace OJS.Data
{
    using System;

    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Data.Contracts;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public interface IOjsData : IDisposable
    {
        IDeletableEntityRepository<Contest> Contests { get; }

        IDeletableEntityRepository<Problem> Problems { get; }

        IDeletableEntityRepository<Event> Events { get; }

        IDeletableEntityRepository<ContestCategory> ContestCategories { get; }

        IDeletableEntityRepository<ContestQuestion> ContestQuestions { get; }

        IDeletableEntityRepository<ContestQuestionAnswer> ContestQuestionAnswers { get; }

        IDeletableEntityRepository<FeedbackReport> FeedbackReports { get; }

        IDeletableEntityRepository<Checker> Checkers { get; }

        IDeletableEntityRepository<ProblemResource> Resources { get; }

        IUsersRepository Users { get; }

        IRepository<IdentityRole> Roles { get; }

        IRepository<Setting> Settings { get; }

        ISubmissionsRepository Submissions { get; }

        IRepository<SubmissionType> SubmissionTypes { get; }

        IDeletableEntityRepository<SourceCode> SourceCodes { get; }

        IRepository<LecturerInContest> LecturersInContests { get; }

        IRepository<LecturerInContestCategory> LecturersInContestCategories { get; }

        IRepository<Ip> Ips { get; }

        IRepository<AccessLog> AccessLogs { get; }

        IOjsDbContext Context { get; }

        int SaveChanges();
    }
}
