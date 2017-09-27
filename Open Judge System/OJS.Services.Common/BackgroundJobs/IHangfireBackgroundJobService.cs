namespace OJS.Services.Common.BackgroundJobs
{
    using System;
    using System.Linq.Expressions;

    public interface IHangfireBackgroundJobService : IService
    {
       void AddOrUpdateRecurringJob(object recurringJobId, Expression<Action> methodCall, string cronExpression);

       void AddOrUpdateRecurringJob<T>(object recurringJobId, Expression<Action<T>> methodCall, string cronExpression);
    }
}