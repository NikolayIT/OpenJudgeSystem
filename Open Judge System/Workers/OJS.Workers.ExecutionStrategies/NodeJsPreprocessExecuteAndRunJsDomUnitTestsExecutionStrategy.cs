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

            this.jsDomModulePath = jsDomModulePath;
            this.jQueryModulePath = jQueryModulePath;
            this.handlebarsModulePath = handlebarsModulePath;
        }
    }
}
