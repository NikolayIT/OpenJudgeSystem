namespace OJS.Web.Common
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class SystemMessageCollection : IEnumerable<SystemMessage>
    {
        private readonly IList<SystemMessage> list;

        public SystemMessageCollection()
        {
            this.list = new List<SystemMessage>();
        }

        public void Add(SystemMessage message)
        {
            this.list.Add(message);
        }

        public void Add(string content, SystemMessageType type, int importance)
        {
            var message = new SystemMessage { Content = content, Importance = importance, Type = type };
            this.Add(message);
        }

        public IEnumerator<SystemMessage> GetEnumerator()
        {
            return this.list.OrderByDescending(x => x.Importance).ThenByDescending(x => x.Type).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
