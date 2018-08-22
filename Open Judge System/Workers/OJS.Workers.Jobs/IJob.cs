namespace OJS.Workers.Jobs
{
    using SimpleInjector;

    public interface IJob
    {
        string Name { get; set; }

        void Start(Container container);

        void Stop();
    }
}