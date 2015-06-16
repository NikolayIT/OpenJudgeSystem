namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using OJS.Workers.Common;
    using OJS.Workers.Checkers;

    public class NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy : NodeJsPreprocessExecuteAndCheckExecutionStrategy
    {
        private string mochaModulePath;
        private string chaiModulePath;

        public NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy(string nodeJsExecutablePath, string mochaModulePath, string chaiModulePath)
            : base(nodeJsExecutablePath)
        {
            if (!File.Exists(nodeJsExecutablePath))
            {
                throw new ArgumentException(string.Format("Mocha not found in: {0}", nodeJsExecutablePath), "mochaModulePath");
            }

            if (!Directory.Exists(chaiModulePath))
            {
                throw new ArgumentException(string.Format("Chai not found in: {0}", nodeJsExecutablePath), "chaiModulePath");
            }

            this.mochaModulePath = mochaModulePath;
            this.chaiModulePath = chaiModulePath.Replace('\\', '/');
        }

        protected override string JsCodeRequiredModules
        {
            get
            {
                return @"
var chai = require('" + chaiModulePath + @"'),
	assert = chai.assert;
	expect = chai.expect,
	should = chai.should();";
            }
        }

        protected override string JsCodePreevaulationCode
        {
            get
            {
                return @"
describe('TestScope', function() {
	it('Test', function(done) {
		var content = '';";
            }
        }

        protected override string JsCodeEvaluation
        {
            get
            {
                return @"
    var inputData = content.trim();
    var result = code.run();
	
	testFunc = new Function('assert', 'expect', 'should', ""var result = this.valueOf();"" + inputData);
    testFunc.call(result, assert, expect, should);
	done();";
            }
        }

        protected override string JsCodePostevaulationCode
        {
            get
            {
                return @"
    });
});";
            }
        }

        protected override List<TestResult> ProcessTests(ExecutionContext executionContext, IExecutor executor, string codeSavePath)
        {
            IChecker checker = Checker.CreateChecker(executionContext.CheckerAssemblyName, executionContext.CheckerTypeName, executionContext.CheckerParameter);

            var testResults = new List<TestResult>();

            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = executor.Execute(this.NodeJsExecutablePath, test.Input, executionContext.TimeLimit, executionContext.MemoryLimit, new[] { this.mochaModulePath, codeSavePath });
                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, processExecutionResult.ReceivedOutput);
                testResults.Add(testResult);
            }

            return testResults;
        }
    }
}
