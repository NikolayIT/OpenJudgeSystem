namespace OJS.Workers.Common.Agents
{
    public class TaskInformation
    {
        public int TimeLimit { get; set; }

        public long MemoryLimit { get; set; }

        public string CheckerDllFile { get; set; }

        public string CheckerClassName { get; set; }
    }
}
