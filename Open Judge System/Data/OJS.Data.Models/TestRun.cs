namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    using OJS.Workers.Common.Models;

    public class TestRun
    {
        [Key]
        public int Id { get; set; }

        public int SubmissionId { get; set; }

        public Submission Submission { get; set; }

        public int TestId { get; set; }

        public Test Test { get; set; }

        public int TimeUsed { get; set; }

        public long MemoryUsed { get; set; }

        public TestRunResultType ResultType { get; set; }

        public string ExecutionComment { get; set; }

        public string CheckerComment { get; set; }

        public string ExpectedOutputFragment { get; set; }

        public string UserOutputFragment { get; set; }
    }
}