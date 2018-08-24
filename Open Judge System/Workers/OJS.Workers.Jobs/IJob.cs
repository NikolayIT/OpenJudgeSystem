namespace OJS.Workers.Jobs
{
    using OJS.Workers.Common;

    public interface IJob
    {
        string Name { get; set; }

        void Start(IDependencyContainer dependencyContainer);

        void Stop();
    }
}