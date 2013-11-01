namespace OJS.Workers.Agent
{
    public interface IFileCache
    {
        byte[] this[string key] { get; set; }

        bool Contains(string key);
    }
}
