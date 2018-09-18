namespace OJS.Workers.Jobs
{
    public interface IJob
    {
        string Name { get; set; }

        void Start();

        void Stop();
    }
}