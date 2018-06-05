namespace OJS.Services.Common.BackgroundJobs
{
    using System;
    using System.Linq.Expressions;

    public interface IHangfireBackgroundJobService : IService
    {
        object AddFireAndForgetJob<T>(Expression<Action<T>> methodCall);

        void AddOrUpdateRecurringJob(object recurringJobId, Expression<Action> methodCall, string cronExpression);

        void AddOrUpdateRecurringJob<T>(object recurringJobId, Expression<Action<T>> methodCall, string cronExpression);

        void OnSucceededStateContinueWith<T>(string parentJobId, Expression<Action<T>> methodCall);
    }
}