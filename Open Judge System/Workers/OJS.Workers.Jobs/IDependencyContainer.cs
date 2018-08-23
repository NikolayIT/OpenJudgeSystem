namespace OJS.Workers.Jobs
{
    using System;

    public interface IDependencyContainer
    {
        T GetInstance<T>() where T : class;

        IDisposable CreateScope();
    }
}