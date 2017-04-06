namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json.Linq;

    public class JsonExecutionResult
    {
        private const string InvalidJsonReplace = "]}[},!^@,Invalid,!^@,{]{[";

        public IList<string> TestsErrors { get; set; }

        public bool Passed { get; set; }

        public string Error { get; set; }

        public int TotalPasses { get; set; }

        public List<int> PassingIndexes { get; set; }

        public int UsersTestCount { get; set; }

        public int InitialPassingTests { get; set; }

        public int TotalTests { get; set; }

        public static JsonExecutionResult Parse(string result, bool forceErrorExtracting = false, bool getTestIndexes = false)
        {
            JObject jsonTestResult = null;
            var totalPasses = 0;
            var totalTests = 0;
            var errors = new List<string>();
            var error = string.Empty;
            var userTestsCount = 0;
            var initialPassedTests = 0;
            var passed = false;
            try
            {
                jsonTestResult = JObject.Parse(result.Trim().Replace("/*", InvalidJsonReplace).Replace("*/", InvalidJsonReplace));
                totalPasses = (int)jsonTestResult["stats"]["passes"];
                totalTests = (int)jsonTestResult["stats"]["tests"];
                passed = totalPasses == 1;
                var testsJs = (JArray)jsonTestResult["tests"];
                for (int i = 0; i < testsJs.Count; i++)
                {
                    var currentTitle = jsonTestResult["tests"][i]["fullTitle"];
                    var stack = jsonTestResult["tests"][i]["err"]["stack"];
                    var token = (string)jsonTestResult["tests"][i]["err"]["message"] ?? string.Empty;
                    var entry = stack != null ? token : null;

                    // By convention the groups of user tests will be separated in describes named "Test 0 ", then "Test 1 " and so forth
                    userTestsCount += currentTitle.ToString().StartsWith("Test 0 ") ? 1 : 0;

                    // The second group (the second zero test) "Test 1 " should be the one holding the correct solution, thus we extract the amount of                    
                    // correct tests in that group, so that the Execution Strategy has a base for judging the other tests
                    initialPassedTests += currentTitle.ToString().StartsWith("Test 1 ") && entry == null ? 1 : 0;
                    errors.Add(entry);
                }
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

            if (forceErrorExtracting)
            {
                try
                {
                    var failures = (JArray)jsonTestResult["failures"];
                    for (int i = 0; i < failures.Count; i++)
                    {
                        error += (string)jsonTestResult["failures"][i]["err"]["message"];
                        error += Environment.NewLine;
                    }
                }
                catch
                {
                    error = "Invalid console output!";
                }
            }

            return new JsonExecutionResult
            {
                TestsErrors = errors,
                Error = error,
                PassingIndexes = testsIndexes,
                TotalPasses = totalPasses,
                TotalTests = totalTests,
                Passed = passed,
                UsersTestCount = userTestsCount,
                InitialPassingTests = initialPassedTests
            };
        }
    }
}
