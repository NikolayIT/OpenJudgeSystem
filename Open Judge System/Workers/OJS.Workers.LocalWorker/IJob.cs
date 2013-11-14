namespace OJS.Workers.LocalWorker
{
    public interface IJob
    {
        string Name { get; set; }

        void Start();

        void Stop();
    }
}
