namespace OJS.Data
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;

    using OJS.Data.Models;

    public interface IOjsDbContext : IDisposable
    {
        IDbSet<Setting> Settings { get; }

        IDbSet<Contest> Contests { get; }

        IDbSet<Problem> Problems { get; }

        IDbSet<News> News { get; }

        IDbSet<Event> Events { get; }

        IDbSet<Participant> Participants { get; }

        IDbSet<ContestCategory> ContestCategories { get; set; }

        IDbSet<Checker> Checkers { get; set; }

        IDbSet<Test> Tests { get; set; }

        IDbSet<Submission> Submissions { get; set; }

        IDbSet<SubmissionType> SubmissionTypes { get; set; }

        IDbSet<FeedbackReport> FeedbackReports { get; set; }

        IDbSet<ParticipantAnswer> ParticipantAnswers { get; set; }

        DbContext DbContext { get; }

        int SaveChanges();

        void ClearDatabase();

        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

        IDbSet<T> Set<T>() where T : class;
    }
}
