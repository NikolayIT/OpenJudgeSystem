namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;

    public class IoJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy
    {
        private string jsdomModulePath;
        private string jqueryModulePath;
        private string handlebarsModulePath;
        private string sinonModulePath;
        private string sinonChaiModulePath;

        public IoJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy(
            string iojsExecutablePath,
            string mochaModulePath,
            string chaiModulePath,
            string jsdomModulePath,
            string jqueryModulePath,
            string handlebarsModulePath,
            string sinonModulePath,
            string sinonChaiModulePath,
            string underscoreModulePath) // TODO: make this modular by getting requires from test
            : base(iojsExecutablePath, mochaModulePath, chaiModulePath, underscoreModulePath)
        {
            if (!Directory.Exists(jsdomModulePath))
            {
                throw new ArgumentException($"jsDom not found in: {jsdomModulePath}", nameof(jsdomModulePath));
            }

            if (!Directory.Exists(jqueryModulePath))
            {
                throw new ArgumentException($"jQuery not found in: {jqueryModulePath}", nameof(jqueryModulePath));
            }

            if (!Directory.Exists(handlebarsModulePath))
            {
                throw new ArgumentException($"Handlebars not found in: {handlebarsModulePath}", nameof(handlebarsModulePath));
            }

            if (!Directory.Exists(sinonModulePath))
            {
                throw new ArgumentException($"Sinon not found in: {sinonModulePath}", nameof(sinonModulePath));
            }

            if (!Directory.Exists(sinonChaiModulePath))
            {
                throw new ArgumentException($"Sinon-chai not found in: {sinonChaiModulePath}", nameof(sinonChaiModulePath));
            }

            this.jsdomModulePath = this.ProcessModulePath(jsdomModulePath);
            this.jqueryModulePath = this.ProcessModulePath(jqueryModulePath);
            this.handlebarsModulePath = this.ProcessModulePath(handlebarsModulePath);
            this.sinonModulePath = this.ProcessModulePath(sinonModulePath);
            this.sinonChaiModulePath = this.ProcessModulePath(sinonChaiModulePath);
        }

        protected override string JsCodeRequiredModules => base.JsCodeRequiredModules + @",
    jsdom = require('" + this.jsdomModulePath + @"'),
    jq = require('" + this.jqueryModulePath + @"'),
    sinon = require('" + this.sinonModulePath + @"'),
    sinonChai = require('" + this.sinonChaiModulePath + @"'),
    handlebars = require('" + this.handlebarsModulePath + @"')";

        protected override string JsCodePreevaulationCode => @"
chai.use(sinonChai);

describe('TestDOMScope', function() {
    before(function(done) {
        jsdom.env({
            html: '',
            done: function(errors, window) {
                global.window = window;
                global.document = window.document;
                global.$ = jq(window);
                global.handlebars = handlebars;
                Object.keys(window)
                    .filter(function (prop) {
                        return prop.toLowerCase().indexOf('html') >= 0;
                    }).forEach(function (prop) {
                        global[prop] = window[prop];
                    });
                done();
            }
        });
    });

	it('Test', function(done) {
		var content = '';";

        protected override string JsCodeTemplate => base.JsCodeTemplate
            .Replace("process.removeListener = undefined;", string.Empty)
            .Replace("setTimeout = undefined;", string.Empty)
            .Replace("delete setTimeout;", string.Empty);

        protected override string TestFuncVariables => base.TestFuncVariables + ", 'sinon', '_'";
    }
}
