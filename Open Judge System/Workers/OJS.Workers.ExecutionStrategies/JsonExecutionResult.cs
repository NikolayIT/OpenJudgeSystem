namespace OJS.Workers.ExecutionStrategies
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;

    public class JsonExecutionResult
    {
        private const string InvalidJsonReplace = "]}[},!^@,Invalid,!^@,{]{[";

        public bool Passed { get; set; }

        public string Error { get; set; }

        public int TotalPasses { get; set; }

        public List<int> PassingIndexes { get; set; }

        public static JsonExecutionResult Parse(string result, bool forceErrorExtracting = false, bool getTestIndexes = false)
        {
            JObject jsonTestResult = null;
            var passed = false;
            string error = null;
            var totalPasses = 0;

            try
            {
                jsonTestResult = JObject.Parse(result.Trim().Replace("/*", InvalidJsonReplace).Replace("*/", InvalidJsonReplace));
                totalPasses = (int)jsonTestResult["stats"]["passes"];
                passed = totalPasses == 1;
            }
            catch
            {
                error = "Invalid console output!";
            }

            var testsIndexes = new List<int>();
            if (getTestIndexes)
            {
                try
                {
                    testsIndexes = jsonTestResult["passing"].Values<int>().ToList();
                }
                catch
                {
                    error = "Invalid console output!";
                }
            }

            if (!passed || forceErrorExtracting)
            {
                try
                {
                    error = (string)jsonTestResult["failures"][0]["err"]["message"];
                }
                catch
                {
                    error = "Invalid console output!";
                }
            }

            return new JsonExecutionResult
            {
                Passed = passed,
                Error = error,
                PassingIndexes = testsIndexes,
                TotalPasses = totalPasses
            };
        }
    }
}
