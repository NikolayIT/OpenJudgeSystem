namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;

    public class NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy
    {
        private string jsDomModulePath;
        private string jQueryModulePath;
        private string handlebarsModulePath;

        public NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy(
            string nodeJsExecutablePath,
            string mochaModulePath, 
            string chaiModulePath, 
            string jsDomModulePath, 
            string jQueryModulePath,
            string handlebarsModulePath)
            : base(nodeJsExecutablePath, mochaModulePath, chaiModulePath)
        {
            if (!Directory.Exists(jsDomModulePath))
            {
                throw new ArgumentException(string.Format("jsDom not found in: {0}", jsDomModulePath), "jsDomModulePath");
            }

            if (!Directory.Exists(jQueryModulePath))
            {
                throw new ArgumentException(string.Format("jQuery not found in: {0}", jQueryModulePath), "jQueryModulePath");
            }

            if (!Directory.Exists(handlebarsModulePath))
            {
                throw new ArgumentException(string.Format("Handlebars not found in: {0}", handlebarsModulePath), "handlebarsModulePath");
            }

            this.jsDomModulePath = this.ProcessModulePath(jsDomModulePath);
            this.jQueryModulePath = this.ProcessModulePath(jQueryModulePath);
            this.handlebarsModulePath = this.ProcessModulePath(handlebarsModulePath);
        }

        protected override string JsCodeRequiredModules
        {
            get
            {
                return base.JsCodeRequiredModules + @",
    jsdom = require('" + this.jsDomModulePath + @"'),
    jq = require('" + this.jQueryModulePath + @"'),
    hs = require('" + this.handlebarsModulePath + @"')";
            }
        }

        protected override string JsCodePreevaulationCode
        {
            get
            {
                return @"
describe('TestDOMScope', function() {
    var htmlTemplate = '<div id=""root""></div>';

    before(function(done) {
        jsdom.env({
            html: '',
            done: function(errors, window) {
                global.window = window;
                global.document = window.document;
                global.$ = jq(window);
                done();
            }
        });
    });

    beforeEach(function() {
       document.body.innerHTML = htmlTemplate;
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
