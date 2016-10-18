namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;

    public class IoJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy
    {
        public IoJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy(
            string nodeJsExecutablePath,
            string mochaModulePath,
            string chaiModulePath,
            string jsdomModulePath,
            string jqueryModulePath,
            string handlebarsModulePath,
            string sinonModulePath,
            string sinonChaiModulePath,
            string underscoreModulePath,
            int baseTimeUsed,
            int baseMemoryUsed) // TODO: make this modular by getting requires from test
            : base(
                  nodeJsExecutablePath,
                  mochaModulePath,
                  chaiModulePath,
                  underscoreModulePath,
                  baseTimeUsed,
                  baseMemoryUsed)
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

            this.JsDomModulePath = this.ProcessModulePath(jsdomModulePath);
            this.JQueryModulePath = this.ProcessModulePath(jqueryModulePath);
            this.HandlebarsModulePath = this.ProcessModulePath(handlebarsModulePath);
            this.SinonModulePath = this.ProcessModulePath(sinonModulePath);
            this.SinonChaiModulePath = this.ProcessModulePath(sinonChaiModulePath);
        }

        protected string JsDomModulePath { get; }

        protected string JQueryModulePath { get; }

        protected string HandlebarsModulePath { get; }

        protected string SinonModulePath { get; }

        protected string SinonChaiModulePath { get; }

        protected override string JsCodeRequiredModules => base.JsCodeRequiredModules + @",
    jsdom = require('" + this.JsDomModulePath + @"'),
    jq = require('" + this.JQueryModulePath + @"'),
    sinon = require('" + this.SinonModulePath + @"'),
    sinonChai = require('" + this.SinonChaiModulePath + @"'),
    handlebars = require('" + this.HandlebarsModulePath + @"')";

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
                Object.getOwnPropertyNames(window)
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
