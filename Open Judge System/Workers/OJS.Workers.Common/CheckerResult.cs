namespace OJS.Workers.Common
{
    public struct CheckerResult
    {
        public bool IsCorrect { get; set; }

        public CheckerResultType ResultType { get; set; }

        /// <summary>
        /// More detailed information visible only by administrators.
        /// </summary>
        public CheckerDetails CheckerDetails { get; set; }
    }
}
