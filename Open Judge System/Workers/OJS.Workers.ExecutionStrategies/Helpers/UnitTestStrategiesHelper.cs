namespace OJS.Workers.ExecutionStrategies.Helpers
{
    using System;
    using System.Text.RegularExpressions;

    using OJS.Workers.Common;

    internal static class UnitTestStrategiesHelper
    {
        public const string TestedCodeFileName = "TestedCode";

        public static readonly string TestedCodeFileNameWithExtension =
            $"{TestedCodeFileName}{Constants.CSharpFileExtension}";

        /// <summary>
        /// Gets the output message and the count of the original tests passed,
        /// by running the provided regex on the received output from the execution process
        /// </summary>
        /// <param name="receivedOutput">The output from the console runner</param>
        /// <param name="testResultsRegexPattern">The Regex pattern used to catch the passing and failing tests</param>
        /// <param name="originalTestsPassed">The number of unit tests that have passed on the first test</param>
        /// <param name="isFirstTest">Bool indicating if the results are for the first test</param>
        public static(string message, int originalTestsPassed) GetTestResult(
            string receivedOutput,
            string testResultsRegexPattern,
            int originalTestsPassed,
            bool isFirstTest)
        {
            ExtractTestResult(receivedOutput, testResultsRegexPattern, out var passedTests, out var totalTests);

            var message = "Test Passed!";

            if (totalTests == 0)
            {
                message = "No tests found";
            }
            else if (passedTests >= originalTestsPassed)
            {
                message = "No functionality covering this test!";
            }

            if (isFirstTest)
            {
                originalTestsPassed = passedTests;

                if (totalTests != passedTests)
                {
                    message = "Not all tests passed on the correct solution.";
                }
                else
                {
                    message = "Test Passed!";
                }
            }

            return (message, originalTestsPassed);
        }

        /// <summary>
        /// Grabs the last match from a match collection,
        /// since the NUnit output is always the last one,
        /// thus ensuring that the tests output is the genuine one,
        /// preventing the user from tampering with it
        /// </summary>
        /// <param name="receivedOutput">The output from the console runner</param>
        /// <param name="testResultsRegexPattern">The Regex pattern used to catch the passing and failing tests</param>
        /// <param name="passedTests">Ouputs the count of passing tests</param>
        /// <param name="totalTests">Ouputs total tests count</param>
        private static void ExtractTestResult(
            string receivedOutput,
            string testResultsRegexPattern,
            out int passedTests,
            out int totalTests)
        {
            var testResultsRegex = new Regex(testResultsRegexPattern);
            var res = testResultsRegex.Matches(receivedOutput);
            if (res.Count == 0)
            {
                throw new ArgumentException("The process did not produce any output!");
            }

            totalTests = int.Parse(res[res.Count - 1].Groups[1].Value);
            passedTests = int.Parse(res[res.Count - 1].Groups[2].Value);
        }
    }
}