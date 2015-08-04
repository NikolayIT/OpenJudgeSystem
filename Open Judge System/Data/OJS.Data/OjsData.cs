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

        public IContestsRepository Contests
        {
            get
            {
                return (ContestsRepository)this.GetRepository<Contest>();
            }
        }

        public IDeletableEntityRepository<Problem> Problems
        {
            get
            {
                return this.GetDeletableEntityRepository<Problem>();
            }
        }

        public ITestRepository Tests
        {
            get
            {
                return (TestRepository)this.GetRepository<Test>();
            }
        }

        public IDeletableEntityRepository<News> News
        {
            get
            {
                return this.GetDeletableEntityRepository<News>();
            }
        }

        public IDeletableEntityRepository<Event> Events
        {
            get
            {
                return this.GetDeletableEntityRepository<Event>();
            }
        }

        public IDeletableEntityRepository<ContestCategory> ContestCategories
        {
            get
            {
                return this.GetDeletableEntityRepository<ContestCategory>();
            }
        }

        public IDeletableEntityRepository<ContestQuestion> ContestQuestions
        {
            get
            {
                return this.GetDeletableEntityRepository<ContestQuestion>();
            }
        }

        public IDeletableEntityRepository<ContestQuestionAnswer> ContestQuestionAnswers
        {
            get
            {
                return this.GetDeletableEntityRepository<ContestQuestionAnswer>();
            }
        }

        public ISubmissionsRepository Submissions
        {
            get
            {
                return (SubmissionsRepository)this.GetRepository<Submission>();
            }
        }

        public IRepository<SubmissionType> SubmissionTypes
        {
            get
            {
                return this.GetRepository<SubmissionType>();
            }
        }

        public IDeletableEntityRepository<SourceCode> SourceCodes
        {
            get
            {
                return this.GetDeletableEntityRepository<SourceCode>();
            }
        }

        public IRepository<AccessLog> UsageLogs
        {
            get
            {
                return this.GetRepository<AccessLog>();
            }
        }

        public ITestRunsRepository TestRuns
        {
            get
            {
                return (TestRunsRepository)this.GetRepository<TestRun>();
            }
        }

        public IParticipantsRepository Participants
        {
            get
            {
                return (ParticipantsRepository)this.GetRepository<Participant>();
            }
        }

        public IDeletableEntityRepository<FeedbackReport> FeedbackReports
        {
            get
            {
                return this.GetDeletableEntityRepository<FeedbackReport>();
            }
        }

        public IDeletableEntityRepository<Checker> Checkers
        {
            get
            {
                return this.GetDeletableEntityRepository<Checker>();
            }
        }

        public IDeletableEntityRepository<ProblemResource> Resources
        {
            get
            {
                return this.GetDeletableEntityRepository<ProblemResource>();
            }
        }

        public IRepository<Setting> Settings
        {
            get
            {
                return this.GetRepository<Setting>();
            }
        }

        public IUsersRepository Users
        {
            get
            {
                return (UsersRepository)this.GetRepository<UserProfile>();
            }
        }

        public IRepository<IdentityRole> Roles
        {
            get
            {
                return this.GetRepository<IdentityRole>();
            }
        }

        public IOjsDbContext Context
        {
            get
            {
                return this.context;
            }
        }

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The number of objects written to the underlying database.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">Thrown if the context has been disposed.</exception>
        public int SaveChanges()
        {
            return this.context.SaveChanges();
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.context != null)
                {
                    this.context.Dispose();
                }
            }
        }

        private IRepository<T> GetRepository<T>() where T : class
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
