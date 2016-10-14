namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;

    public class NodeJsPreprocessExecuteAndRunUnitTestsWithStubbedRequestsUsingSinonAndMochaExecutionStrategy : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy
    {
        public NodeJsPreprocessExecuteAndRunUnitTestsWithStubbedRequestsUsingSinonAndMochaExecutionStrategy(
           string nodeJsExecutablePath,
           string mochaModulePath,
           string chaiModulePath,
           string jsdomModulePath,
           string jqueryModulePath,
           string handlebarsModulePath,
           string sinonJsDomModulePath,
           string sinonChaiModulePath,
           string underscoreModulePath,
           int baseTimeUsed,
           int baseMemoryUsed)
            : base(nodeJsExecutablePath, mochaModulePath, chaiModulePath, underscoreModulePath, baseTimeUsed, baseMemoryUsed)
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

            if (!File.Exists(sinonJsDomModulePath))
            {
                throw new ArgumentException($"Sinon not found in: {sinonJsDomModulePath}", nameof(sinonJsDomModulePath));
            }

            if (!Directory.Exists(sinonChaiModulePath))
            {
                throw new ArgumentException($"Sinon-chai not found in: {sinonChaiModulePath}", nameof(sinonChaiModulePath));
            }

            this.JsDomModulePath = this.ProcessModulePath(jsdomModulePath);
            this.JQueryModulePath = this.ProcessModulePath(jqueryModulePath);
            this.HandlebarsModulePath = this.ProcessModulePath(handlebarsModulePath);
            this.SinonChaiModulePath = this.ProcessModulePath(sinonChaiModulePath);
            this.SinonJsDomModulePath = this.ProcessModulePath(sinonJsDomModulePath);
        }

        protected string JsDomModulePath { get; }

        protected string JQueryModulePath { get; }

        protected string HandlebarsModulePath { get; }

        protected string SinonChaiModulePath { get; }

        // TODO Might completely swap IoJs Execution Strategy for this one
        // Full version of the sinon module for use in the jsdom, will not work with NodeJs's require
        protected string SinonJsDomModulePath { get; }

        protected override string JsCodeRequiredModules => base.JsCodeRequiredModules + @",
    jsdom = require('" + this.JsDomModulePath + @"'),
    jq = require('" + this.JQueryModulePath + @"'),
    sinonChai = require('" + this.SinonChaiModulePath + @"'),
    handlebars = require('" + this.HandlebarsModulePath + @"'),
    fs = require('fs'),
    sinonJsDom = fs.readFileSync('" + this.SinonJsDomModulePath + @"','utf-8')";

        protected override string JsCodePreevaulationCode => @"
chai.use(sinonChai);
fs = undefined;

describe('TestDOMScope', function() {
    before(function(done) {
        jsdom.env({
            html: '',
            src:[sinonJsDom],
            done: function(errors, window) {
                global.window = window;
                global.document = window.document;
                global.$ = jq(window);
                global.sinon = window.sinon;
                global.handlebars = handlebars;
                Object.getOwnPropertyNames(window)
                    .filter(function (prop) {
                        return prop.toLowerCase().indexOf('html') >= 0;
                    }).forEach(function (prop) {
                        global[prop] = window[prop];
                    });

                global.server = sinon.fakeServer.create();
                server.autoRespond = true;

                done();
            }
        });
    });

	it('Test', function(done) {
		var content = '';";

        protected override string JsCodePostevaulationCode => @"
    });

    after(function(done){
        server.restore();
        done();
    });
});";

        protected override string JsCodeTemplate => base.JsCodeTemplate
           .Replace("process.removeListener = undefined;", string.Empty)
           .Replace("setTimeout = undefined;", string.Empty)
           .Replace("delete setTimeout;", string.Empty);

        protected override string TestFuncVariables => base.TestFuncVariables + ", 'done', '_'";
    }
}
