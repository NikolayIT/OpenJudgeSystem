namespace OJS.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using OJS.Data.Contracts;
    using OJS.Workers.Common.Extensions;

    public class Test : IOrderable
    {
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
            get => this.InputData.Decompress();

            set => this.InputData = value.Compress();
        }

        /// <remarks>
        /// Using byte[] (compressed with zip) to save database space.
        /// </remarks>
        public byte[] OutputData { get; set; }

        [NotMapped]
        public string OutputDataAsString
        {
            get => this.OutputData.Decompress();

            set => this.OutputData = value.Compress();
        }

        public bool IsTrialTest { get; set; }

        public bool IsOpenTest { get; set; }

        public bool HideInput { get; set; }

        public int OrderBy { get; set; }

        public virtual ICollection<TestRun> TestRuns { get; set; } = new HashSet<TestRun>();
    }
}
