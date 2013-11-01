namespace OJS.Workers.Common.Communication
{
    using System;

    [Serializable]
    public class NetworkDataObject
    {
        public NetworkDataObjectType Type { get; set; }

        public object Data { get; set; }
    }
}
