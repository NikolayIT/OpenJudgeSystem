namespace OJS.Workers.ExecutionStrategies
{
    public class RubyExecutionStrategy : ExecutionStrategy
    {

        public RubyExecutionStrategy(string rubyPath)
        {
            this.RubyPath = rubyPath;
        }

        public string RubyPath { get; set; }

        public override ExecutionResult Execute(ExecutionContext executionContext)
        {
            throw new System.NotImplementedException();
        }
    }
}
