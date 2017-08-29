namespace OJS.Common.BackgroundJobs.Contracts
{
    using System;
    using System.Linq.Expressions;

    public interface IBackgroundJobService
    {
       void AddOrUpdateRecurringJob(object recurringJobId, Expression<Action> methodCall, string cronExpression);
    }
}
