namespace OJS.Workers.Agent
{
    using OJS.Workers.Common.Communication;

    public interface IJobWorker
    {
        JobResult DoJob(Job job);
    }
}
