namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;

    public class IoJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy
    {
        private string jsdomModulePath;
        private string jqueryModulePath;
        private string handlebarsModulePath;

        public IoJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy(
            string iojsExecutablePath,
            string mochaModulePath, 
            string chaiModulePath, 
            string jsdomModulePath, 
            string jqueryModulePath,
            string handlebarsModulePath)
            : base(iojsExecutablePath, mochaModulePath, chaiModulePath)
        {
            if (!Directory.Exists(jsdomModulePath))
            {
                throw new ArgumentException(string.Format("jsDom not found in: {0}", jsdomModulePath), "jsDomModulePath");
            }

            if (!Directory.Exists(jqueryModulePath))
            {
                throw new ArgumentException(string.Format("jQuery not found in: {0}", jqueryModulePath), "jQueryModulePath");
            }

            if (!Directory.Exists(handlebarsModulePath))
            {
                throw new ArgumentException(string.Format("Handlebars not found in: {0}", handlebarsModulePath), "handlebarsModulePath");
            }

            this.jsdomModulePath = this.ProcessModulePath(jsdomModulePath);
            this.jqueryModulePath = this.ProcessModulePath(jqueryModulePath);
            this.handlebarsModulePath = this.ProcessModulePath(handlebarsModulePath);
        }

        protected override string JsCodeRequiredModules
        {
            get
            {
                return base.JsCodeRequiredModules + @",
    jsdom = require('" + this.jsdomModulePath + @"'),
    jq = require('" + this.jqueryModulePath + @"'),
    handlebars = require('" + this.handlebarsModulePath + @"')";
            }
        }

        protected override string JsCodePreevaulationCode
        {
            get
            {
                return @"
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
            }
        }

        protected override string JsCodeTemplate
        {
            get
            {
                return base.JsCodeTemplate
                    .Replace("process.removeListener = undefined;", string.Empty)
                    .Replace("setTimeout = undefined;", string.Empty)
                    .Replace("delete setTimeout;", string.Empty);
            }
        }
    }
}
