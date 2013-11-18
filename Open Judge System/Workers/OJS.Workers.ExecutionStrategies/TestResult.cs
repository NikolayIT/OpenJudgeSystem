namespace OJS.Workers.ExecutionStrategies
{
    using OJS.Common.Models;

    public class TestResult
    {
        public int Id { get; set; }

        public TestRunResultType ResultType { get; set; }

        public int TimeUsed { get; set; }

        public int MemoryUsed { get; set; }

        public string ExecutionComment { get; set; }

        public string CheckerComment { get; set; }
    }
}