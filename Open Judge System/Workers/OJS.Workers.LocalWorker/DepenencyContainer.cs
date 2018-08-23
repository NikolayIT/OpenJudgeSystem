namespace OJS.Workers.LocalWorker
{
    using System;
    using OJS.Workers.Jobs;

    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    public class DepenencyContainer : Container, IDependencyContainer
    {
        public new T GetInstance<T>()
            where T : class => base.GetInstance<T>();

        public IDisposable CreateScope() => ThreadScopedLifestyle.BeginScope(this);
    }
}