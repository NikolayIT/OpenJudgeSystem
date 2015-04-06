namespace OJS.Workers.ExecutionStrategies
{
    using OJS.Common.Models;
    using OJS.Workers.Common;

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