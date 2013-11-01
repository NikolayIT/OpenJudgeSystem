namespace OJS.Workers.Common.Agents
{
    using System.Collections.Generic;

    public class AgentResponseData
    {
        public CompileResult CompileResult { get; set; }

        public ProcessExecutionResult ExecutionResult { get; set; }

        public List<TestResult> TestResults { get; set; }
    }
}
