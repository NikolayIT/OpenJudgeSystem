namespace OJS.Services.Common.BackgroundJobs
{
    using System;
    using System.Linq.Expressions;

    using Hangfire;

    using OJS.Services.Common.BackgroundJobs.Contracts;
    public class HangfireBackgroundJobService : IBackgroundJobService
    {
        public void AddOrUpdateRecurringJob(
            object recurringJobId,
            Expression<Action> methodCall,
            string cronExpression) => RecurringJob.AddOrUpdate((string)recurringJobId, methodCall, cronExpression);
    }
}
