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

        IDbSet<ExamGroup> ExamGroups { get; }

        IDbSet<Problem> Problems { get; }

        IDbSet<ProblemGroup> ProblemGroups { get; }

        IDbSet<Event> Events { get; }

        IDbSet<Participant> Participants { get; }

        IDbSet<ParticipantScore> ParticipantScores { get; }

        IDbSet<ContestCategory> ContestCategories { get; set; }

        IDbSet<ContestQuestion> ContestQuestions { get; set; }

        IDbSet<ContestQuestionAnswer> ContestQuestionAnswers { get; set; }

        IDbSet<Checker> Checkers { get; set; }

        IDbSet<Test> Tests { get; set; }

        IDbSet<Submission> Submissions { get; set; }

        IDbSet<SubmissionForProcessing> SubmissionsForProcessing { get; set; }

        IDbSet<SubmissionType> SubmissionTypes { get; set; }

        IDbSet<SourceCode> SourceCodes { get; set; }

        IDbSet<FeedbackReport> FeedbackReports { get; set; }

        IDbSet<ParticipantAnswer> ParticipantAnswers { get; set; }

        IDbSet<LecturerInContest> LecturersInContests { get; set; }

        IDbSet<LecturerInContestCategory> LecturersInContestCategories { get; set; }

        IDbSet<Ip> Ips { get; set; }

        IDbSet<AccessLog> AccessLogs { get; set; }

        DbContext DbContext { get; }

        int SaveChanges();

        void ClearDatabase();

        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity)
            where TEntity : class;

        IDbSet<T> Set<T>()
            where T : class;
    }
}
