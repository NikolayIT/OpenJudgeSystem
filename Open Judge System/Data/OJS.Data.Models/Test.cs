namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Common.Extensions;
    using OJS.Data.Contracts;

    public class Test : IOrderable
    {
        private ICollection<TestRun> testRuns;

        public Test()
        {
            this.testRuns = new HashSet<TestRun>();
        }

        [Key]
        public int Id { get; set; }

        public int ProblemId { get; set; }

        public virtual Problem Problem { get; set; }

        /// <remarks>
        /// Using byte[] (compressed with zip) to save database space.
        /// </remarks>
        public byte[] InputData { get; set; }

        [NotMapped]
        public string InputDataAsString
        {
            get
            {
                return this.InputData.Decompress();
            }

            set
            {
                this.InputData = value.Compress();
            }
        }

        /// <remarks>
        /// Using byte[] (compressed with zip) to save database space.
        /// </remarks>
        public byte[] OutputData { get; set; }

        [NotMapped]
        public string OutputDataAsString
        {
            get
            {
                return this.OutputData.Decompress();
            }

            set
            {
                this.OutputData = value.Compress();
            }
        }

        public bool IsTrialTest { get; set; }

        public int OrderBy { get; set; }

        public virtual ICollection<TestRun> TestRuns
        {
            get { return this.testRuns; }
            set { this.testRuns = value; }
        }
    }
}
