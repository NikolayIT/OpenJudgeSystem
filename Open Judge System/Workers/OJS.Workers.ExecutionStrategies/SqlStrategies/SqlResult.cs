namespace OJS.Workers.ExecutionStrategies.SqlStrategies
{
    using System.Collections.Generic;

    public class SqlResult
    {
        public SqlResult()
        {
            this.Results = new List<string>();
        }

        public bool Completed { get; set; }

        public ICollection<string> Results { get; set; }
    }
}
