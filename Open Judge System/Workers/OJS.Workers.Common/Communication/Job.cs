namespace OJS.Workers.Common.Communication
{
    using System;

    [Serializable]
    public class Job
    {
        public bool ContainsTaskData { get; set; }

        public byte[] TaskData { get; set; }

        public byte[] UserSolution { get; set; }
    }
}
