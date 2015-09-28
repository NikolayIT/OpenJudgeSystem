namespace SandboxTarget
{
    using System;

    internal class TryToExecuteParams
    {
        private readonly Action<string> action;

        private readonly string name;

        private readonly string parameter;

        public TryToExecuteParams(Action<string> action, string name, string parameter)
        {
            this.action = action;
            this.name = name;
            this.parameter = parameter;
        }

        public Action<string> Action
        {
            get
            {
                return this.action;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public string Parameter
        {
            get
            {
                return this.parameter;
            }
        }
    }
}