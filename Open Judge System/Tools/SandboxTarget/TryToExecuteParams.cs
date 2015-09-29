namespace SandboxTarget
{
    using System;

    internal class TryToExecuteParams
    {
        public TryToExecuteParams(Action<string> action, string name, string parameter)
        {
            this.Action = action;
            this.Name = name;
            this.Parameter = parameter;
        }

        public Action<string> Action { get; }

        public string Name { get; }

        public string Parameter { get; }
    }
}
