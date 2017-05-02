namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using OJS.Common.Extensions;
    using OJS.Workers.Common;

    public class NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy : NodeJsPreprocessExecuteAndCheckExecutionStrategy
    {
        protected const string TestsPlaceholder = "#testsCode#";

        public NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy(
            string nodeJsExecutablePath,
            string mochaModulePath,
            string chaiModulePath,
            string underscoreModulePath,
            int baseTimeUsed,
            int baseMemoryUsed)
            : base(nodeJsExecutablePath, underscoreModulePath, baseTimeUsed, baseMemoryUsed)
        {
            if (!File.Exists(mochaModulePath))
            {
                throw new ArgumentException(
                    $"Mocha not found in: {mochaModulePath}",
                    nameof(mochaModulePath));
            }

            if (!Directory.Exists(chaiModulePath))
            {
                throw new ArgumentException(
                    $"Chai not found in: {chaiModulePath}",
                    nameof(chaiModulePath));
            }

            this.MochaModulePath = mochaModulePath;
            this.ChaiModulePath = FileHelpers.ProcessModulePath(chaiModulePath);
        }

        protected string MochaModulePath { get; }

        protected string ChaiModulePath { get; }

        protected override string JsCodeRequiredModules => base.JsCodeRequiredModules + @",
    chai = require('" + this.ChaiModulePath + @"'),
	assert = chai.assert,
	expect = chai.expect,
	should = chai.should()";

        protected override string JsCodePreevaulationCode => @"
describe('TestScope', function() {
    let code = {
        run: " + UserInputPlaceholder + @"
    };

    let result = code.run;
    let bgCoderConsole = {};   

    before(function() {
        Object.keys(console)
            .forEach(function (prop) {
                bgCoderConsole[prop] = console[prop];
                console[prop] = new Function('');
            });
    });

    after(function() {
        Object.keys(bgCoderConsole)
            .forEach(function (prop) {
                console[prop] = bgCoderConsole[prop];
            });
    });";

        protected override string JsCodeEvaluation => @"
	it('Test', function(done) {
		let content = '';
        process.stdin.resume();
        process.stdin.on('data', function(buf) { content += buf.toString(); });
        process.stdin.on('end', function() {    
            let inputData = content.trim();

	        let testFunc = new Function('result', " + this.TestFuncVariables + @", inputData);
            testFunc.call({}, result,  " + this.TestFuncVariables.Replace("'", string.Empty) + @");

	        done();
        });
    });";

        protected override string JsCodePostevaulationCode => @"
});";

        protected virtual string TestFuncVariables => "'assert', 'expect', 'should'";

        protected override List<TestResult> ProcessTests(
           ExecutionContext executionContext,
           IExecutor executor,
           IChecker checker,
           string codeSavePath)
        {
            var testResults = new List<TestResult>();

            var arguments = new List<string>();
            arguments.Add(this.MochaModulePath);
            arguments.Add(codeSavePath);
            arguments.AddRange(executionContext.AdditionalCompilerArguments.Split(' '));

            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = this.ExecuteNodeJsProcess(executionContext, executor, test.Input, arguments);
                var mochaResult = JsonExecutionResult.Parse(processExecutionResult.ReceivedOutput);

                var message = "yes";
                if (mochaResult.Error != string.Empty)
                {
                    message = mochaResult.Error;
                }
                else if (mochaResult.TotalPassingTests != 1)
                {
                    message = $"Unexpected error: {mochaResult.TestErrors[0]}";
                }

                var testResult = this.ExecuteAndCheckTest(
                    test,
                    processExecutionResult,
                    checker,
                    message);
                testResults.Add(testResult);
            }

            return testResults;
        }
    }
}
