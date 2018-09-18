namespace OJS.Workers.Common
{
    using System;

    public interface IDependencyContainer
    {
        T GetInstance<T>()
            where T : class;

        IDisposable BeginDefaultScope();
    }
}