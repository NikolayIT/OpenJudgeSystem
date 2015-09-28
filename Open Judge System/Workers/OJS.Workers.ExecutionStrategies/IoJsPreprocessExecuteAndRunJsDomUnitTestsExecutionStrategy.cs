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
        private string underscoreModulePath;

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
            : base(iojsExecutablePath, mochaModulePath, chaiModulePath)
        {
            if (!Directory.Exists(jsdomModulePath))
            {
                throw new ArgumentException(string.Format("jsDom not found in: {0}", jsdomModulePath), "jsdomModulePath");
            }

            if (!Directory.Exists(jqueryModulePath))
            {
                throw new ArgumentException(string.Format("jQuery not found in: {0}", jqueryModulePath), "jqueryModulePath");
            }

            if (!Directory.Exists(handlebarsModulePath))
            {
                throw new ArgumentException(string.Format("Handlebars not found in: {0}", handlebarsModulePath), "handlebarsModulePath");
            }

            if (!Directory.Exists(sinonModulePath))
            {
                throw new ArgumentException(string.Format("Sinon not found in: {0}", sinonModulePath), "handlebarsModulePath");
            }

            if (!Directory.Exists(sinonChaiModulePath))
            {
                throw new ArgumentException(string.Format("Sinon-chai not found in: {0}", sinonChaiModulePath), "handlebarsModulePath");
            }

            if (!Directory.Exists(underscoreModulePath))
            {
                throw new ArgumentException(string.Format("Underscore not found in: {0}", underscoreModulePath), "handlebarsModulePath");
            }

            this.jsdomModulePath = this.ProcessModulePath(jsdomModulePath);
            this.jqueryModulePath = this.ProcessModulePath(jqueryModulePath);
            this.handlebarsModulePath = this.ProcessModulePath(handlebarsModulePath);
            this.sinonModulePath = this.ProcessModulePath(sinonModulePath);
            this.sinonChaiModulePath = this.ProcessModulePath(sinonChaiModulePath);
            this.underscoreModulePath = this.ProcessModulePath(underscoreModulePath);
        }

        protected override string JsCodeRequiredModules
        {
            get
            {
                return base.JsCodeRequiredModules + @",
    jsdom = require('" + this.jsdomModulePath + @"'),
    jq = require('" + this.jqueryModulePath + @"'),
    sinon = require('" + this.sinonModulePath + @"'),
    sinonChai = require('" + this.sinonChaiModulePath + @"'),
    _ = require('" + this.underscoreModulePath + @"'),
    handlebars = require('" + this.handlebarsModulePath + @"')";
            }
        }

        protected override string JsCodePreevaulationCode
        {
            get
            {
                return @"
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

        protected override string TestFuncVariables
        {
            get { return base.TestFuncVariables + ", 'sinon', '_'"; }
        }
    }
}
