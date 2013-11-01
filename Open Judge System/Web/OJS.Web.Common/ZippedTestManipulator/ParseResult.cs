namespace OJS.Web.Common.ZippedTestManipulator
{
    using System.Collections.Generic;

    public class TestsParseResult
    {
        public TestsParseResult()
        {
            this.ZeroInputs = new List<string>();
            this.ZeroOutputs = new List<string>();
            this.Inputs = new List<string>();
            this.Outputs = new List<string>();
        }

        public List<string> ZeroInputs { get; set; }

        public List<string> ZeroOutputs { get; set; }

        public List<string> Inputs { get; set; }

        public List<string> Outputs { get; set; }
    }
}
