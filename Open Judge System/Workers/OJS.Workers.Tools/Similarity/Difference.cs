namespace OJS.Workers.Tools.Similarity
{
    /// <summary>
    /// Details of one difference.
    /// </summary>
    public struct Difference
    {
        /// <summary>
        /// Start Line number in Data A.
        /// </summary>
        public int StartA { get; set; }

        /// <summary>
        /// Start Line number in Data B.
        /// </summary>
        public int StartB { get; set; }

        /// <summary>
        /// Number of changes in Data A.
        /// </summary>
        public int DeletedA { get; set; }

        /// <summary>
        /// Number of changes in Data B.
        /// </summary>
        public int InsertedB { get; set; }
    }
}
