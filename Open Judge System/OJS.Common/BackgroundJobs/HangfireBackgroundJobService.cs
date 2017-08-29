namespace OJS.Common.BackgroundJobs
{
    using System;
    using System.Linq.Expressions;

    using global::Hangfire;
    using OJS.Common.BackgroundJobs.Contracts;

    public class HangfireBackgroundJobService : IBackgroundJobService
    {
        public void AddOrUpdateRecurringJob(object recurringJobId, Expression<Action> methodCall, string cronExpression)
            => RecurringJob.AddOrUpdate((string)recurringJobId, methodCall, cronExpression);
    }
}
