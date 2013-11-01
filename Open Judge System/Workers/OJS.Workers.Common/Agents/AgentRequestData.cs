namespace OJS.Workers.Common.Agents
{
    using System.Collections.Generic;

    public class AgentTaskData
    {
        public List<SourceFile> SourceFiles { get; set; }

        public TaskInformation TaskInformation { get; set; }

        public List<Test> Tests { get; set; }
    }
}
