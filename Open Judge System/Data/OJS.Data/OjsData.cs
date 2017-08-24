namespace OJS.Data
{
    using System;
    using System.Collections.Generic;

    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Data.Contracts;
    using OJS.Data.Models;
    using OJS.Data.Repositories;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class OjsData : IOjsData
    {
        private readonly IOjsDbContext context;

        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();

        public OjsData()
            : this(new OjsDbContext())
        {
        }

        public OjsData(IOjsDbContext context)
        {
            this.context = context;
        }

        public IContestsRepository Contests => (ContestsRepository)this.GetRepository<Contest>();

        public IDeletableEntityRepository<Problem> Problems => this.GetDeletableEntityRepository<Problem>();

        public ITestRepository Tests => (TestRepository)this.GetRepository<Test>();

        public IDeletableEntityRepository<News> News => this.GetDeletableEntityRepository<News>();

        public IDeletableEntityRepository<Event> Events => this.GetDeletableEntityRepository<Event>();

        public IDeletableEntityRepository<ContestCategory> ContestCategories => 
            this.GetDeletableEntityRepository<ContestCategory>();

        public IDeletableEntityRepository<ContestQuestion> ContestQuestions => 
            this.GetDeletableEntityRepository<ContestQuestion>();

        public IDeletableEntityRepository<ContestQuestionAnswer> ContestQuestionAnswers => 
            this.GetDeletableEntityRepository<ContestQuestionAnswer>();

        public ISubmissionsRepository Submissions => (SubmissionsRepository)this.GetRepository<Submission>();

        public IRepository<SubmissionsForProcessing> SubmissionsForProcessing => this
            .GetRepository<SubmissionsForProcessing>();

        public IRepository<SubmissionType> SubmissionTypes => this.GetRepository<SubmissionType>();

        public IDeletableEntityRepository<SourceCode> SourceCodes => this.GetDeletableEntityRepository<SourceCode>();

        public IRepository<LecturerInContest> LecturersInContests => this.GetRepository<LecturerInContest>();

        public IRepository<LecturerInContestCategory> LecturersInContestCategories => this.GetRepository<LecturerInContestCategory>();

        public IRepository<Ip> Ips => this.GetRepository<Ip>();

        public IRepository<AccessLog> AccessLogs => this.GetRepository<AccessLog>();

        public ITestRunsRepository TestRuns => (TestRunsRepository)this.GetRepository<TestRun>();

        public IParticipantsRepository Participants => (ParticipantsRepository)this.GetRepository<Participant>();

        public IParticipantScoresRepository ParticipantScores => (ParticipantScoresRepository)this.GetRepository<ParticipantScore>();

        public IDeletableEntityRepository<FeedbackReport> FeedbackReports => 
            this.GetDeletableEntityRepository<FeedbackReport>();

        public IDeletableEntityRepository<Checker> Checkers => this.GetDeletableEntityRepository<Checker>();

        public IDeletableEntityRepository<ProblemResource> Resources => 
            this.GetDeletableEntityRepository<ProblemResource>();

        public IRepository<Setting> Settings => this.GetRepository<Setting>();

        public IUsersRepository Users => (UsersRepository)this.GetRepository<UserProfile>();

        public IRepository<IdentityRole> Roles => this.GetRepository<IdentityRole>();

        public IOjsDbContext Context => this.context;

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The number of objects written to the underlying database.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">Thrown if the context has been disposed.</exception>
        public int SaveChanges() => this.context.SaveChanges();

        public void Dispose() => this.Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.context?.Dispose();
            }
        }

        private IRepository<T> GetRepository<T>()
            where T : class
        {
            if (!this.repositories.ContainsKey(typeof(T)))
            {
                var type = typeof(GenericRepository<T>);

                if (typeof(T).IsAssignableFrom(typeof(Contest)))
                {
                    type = typeof(ContestsRepository);
                }

                if (typeof(T).IsAssignableFrom(typeof(Submission)))
                {
                    type = typeof(SubmissionsRepository);
                }

                if (typeof(T).IsAssignableFrom(typeof(Test)))
                {
                    type = typeof(TestRepository);
                }

                if (typeof(T).IsAssignableFrom(typeof(TestRun)))
                {
                    type = typeof(TestRunsRepository);
                }

                if (typeof(T).IsAssignableFrom(typeof(UserProfile)))
                {
                    type = typeof(UsersRepository);
                }

                if (typeof(T).IsAssignableFrom(typeof(Participant)))
                {
                    type = typeof(ParticipantsRepository);
                }

                if (typeof(T).IsAssignableFrom(typeof(ParticipantScore)))
                {
                    type = typeof(ParticipantScoresRepository);
                }

                this.repositories.Add(typeof(T), Activator.CreateInstance(type, this.context));
            }

            return (IRepository<T>)this.repositories[typeof(T)];
        }

        private IDeletableEntityRepository<T> GetDeletableEntityRepository<T>()
            where T : class, IDeletableEntity, new()
        {
            if (!this.repositories.ContainsKey(typeof(T)))
            {
                var type = typeof(DeletableEntityRepository<T>);
                this.repositories.Add(typeof(T), Activator.CreateInstance(type, this.context));
            }

            return (IDeletableEntityRepository<T>)this.repositories[typeof(T)];
        }
    }
}
