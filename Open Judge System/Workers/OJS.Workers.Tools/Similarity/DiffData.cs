namespace OJS.Workers.Tools.Similarity
{
    /// <summary>
    /// Data on one input file being compared.
    /// </summary>
    internal class DiffData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiffData"/> class.
        /// </summary>
        /// <param name="initData">reference to the buffer</param>
        internal DiffData(int[] initData)
        {
            this.Data = initData;
            this.Length = initData.Length;
            this.Modified = new bool[this.Length + 2];
        }

        /// <summary>
        /// Number of elements (lines).
        /// </summary>
        internal int Length { get; set; }

        /// <summary>
        /// Buffer of numbers that will be compared.
        /// </summary>
        internal int[] Data { get; set; }

        /// <summary>
        /// Array of booleans that flag for modified data.
        /// This is the result of the diff.
        /// This means deletedA in the first Data or inserted in the second Data.
        /// </summary>
        internal bool[] Modified { get; set; }
    }
}
