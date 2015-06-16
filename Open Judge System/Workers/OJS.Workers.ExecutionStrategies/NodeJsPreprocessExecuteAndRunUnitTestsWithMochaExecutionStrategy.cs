namespace OJS.Workers.ExecutionStrategies
{
    public class NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy : NodeJsPreprocessExecuteAndCheckExecutionStrategy
    {
        public NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy(string mochaExecutablePath)
            : base(mochaExecutablePath)
        {
        }

        protected override string NotFoundErrorMessage
        {
            get
            {
                return "Mocha not found in: {0}";
            }
        }

        protected override string JsCodeRequiredModules
        {
            get
            {
                return @"
var chai = require('chai'),
	assert = chai.assert;
	expect = chai.expect,
	should = chai.should();";
            }
        }

        protected override string JsCodeEvaluation
        {
            get
            {
                return @"
    var inputData = content.trim();
    var result = code.run();
	
	var testFunc = new Function('assert', 'expect', 'should', ""var result = this.valueOf(); "" + inputData)
	it('Test', function() {
		testFunc.call(result, assert, expect, should);
	});";
            }
        }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            throw new System.NotImplementedException();
        }
    }
}
