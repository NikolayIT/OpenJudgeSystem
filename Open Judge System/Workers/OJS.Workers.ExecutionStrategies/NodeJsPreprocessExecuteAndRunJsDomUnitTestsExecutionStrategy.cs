namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;

    public class NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy
    {
        private string jsDomModulePath;
        private string jQueryModulePath;

        public NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy(
            string nodeJsExecutablePath,
            string mochaModulePath, 
            string chaiModulePath, 
            string jsDomModulePath, 
            string jQueryModulePath)
            : base(nodeJsExecutablePath, mochaModulePath, chaiModulePath)
        {
            if (!File.Exists(jsDomModulePath))
            {
                throw new ArgumentException(string.Format("jsDom not found in: {0}", jsDomModulePath), "jsDomModulePath");
            }

            if (!File.Exists(jQueryModulePath))
            {
                throw new ArgumentException(string.Format("jQuery not found in: {0}", jQueryModulePath), "jQueryModulePath");
            }

            this.jsDomModulePath = jsDomModulePath;
            this.jQueryModulePath = jQueryModulePath;
        }
    }
}
