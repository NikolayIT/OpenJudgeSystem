namespace OJS.Workers.ExecutionStrategies
{
    using OJS.Workers.Common;
    using OJS.Workers.Common.Models;

    public class TestResult
    {
        public int Id { get; set; }

        public TestRunResultType ResultType { get; set; }

        public int TimeUsed { get; set; }

        public int MemoryUsed { get; set; }

        public string ExecutionComment { get; set; }

        public CheckerDetails CheckerDetails { get; set; }
    }
}