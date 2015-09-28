namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using OJS.Workers.Common;

    public class NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy : NodeJsPreprocessExecuteAndCheckExecutionStrategy
    {
        private readonly string mochaModulePath;
        private readonly string chaiModulePath;

        public NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy(string nodeJsExecutablePath, string mochaModulePath, string chaiModulePath)
            : base(nodeJsExecutablePath)
        {
            if (!File.Exists(mochaModulePath))
            {
                throw new ArgumentException(string.Format("Mocha not found in: {0}", mochaModulePath), "mochaModulePath");
            }

            if (!Directory.Exists(chaiModulePath))
            {
                throw new ArgumentException(string.Format("Chai not found in: {0}", chaiModulePath), "chaiModulePath");
            }

            this.mochaModulePath = mochaModulePath;
            this.chaiModulePath = this.ProcessModulePath(chaiModulePath);
        }

        protected override string JsCodeRequiredModules
        {
            get
            {
                return @"
var chai = require('" + this.chaiModulePath + @"'),
	assert = chai.assert,
	expect = chai.expect,
	should = chai.should()";
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
    if (result == undefined) {
        result = 'Invalid!';
    }
	
    var bgCoderConsole = {};      
    Object.keys(console)
        .forEach(function (prop) {
            bgCoderConsole[prop] = console[prop];
            console[prop] = new Function('');
        });

	testFunc = new Function(" + this.TestFuncVariables + @", ""var result = this.valueOf();"" + inputData);
    testFunc.call(result, " + this.TestFuncVariables.Replace("'", string.Empty) + @");

    Object.keys(bgCoderConsole)
        .forEach(function (prop) {
            console[prop] = bgCoderConsole[prop];
        });

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

        protected virtual string TestFuncVariables
        {
            get { return "'assert', 'expect', 'should'"; }
        }

        protected override List<TestResult> ProcessTests(ExecutionContext executionContext, IExecutor executor, IChecker checker, string codeSavePath)
        {
            var testResults = new List<TestResult>();

            var arguments = new List<string>();
            arguments.Add(this.mochaModulePath);
            arguments.Add(codeSavePath);
            arguments.AddRange(executionContext.AdditionalCompilerArguments.Split(' '));

            foreach (var test in executionContext.Tests)
            {
                var processExecutionResult = executor.Execute(this.NodeJsExecutablePath, test.Input, executionContext.TimeLimit, executionContext.MemoryLimit, arguments);
                var mochaResult = JsonExecutionResult.Parse(processExecutionResult.ReceivedOutput);
                var testResult = this.ExecuteAndCheckTest(test, processExecutionResult, checker, mochaResult.Passed ? "yes" : string.Format("Unexpected error: {0}", mochaResult.Error));
                testResults.Add(testResult);
            }

            return testResults;
        }

        protected string ProcessModulePath(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
