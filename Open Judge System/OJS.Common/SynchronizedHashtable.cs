namespace OJS.Common
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;

    public class SynchronizedHashtable
    {
        private readonly Hashtable hashtable;

        public SynchronizedHashtable()
        {
            var unSynchronizedHashtable = new Hashtable();
            this.hashtable = Hashtable.Synchronized(unSynchronizedHashtable);
        }

        public bool Contains(object value)
        {
            return this.hashtable.ContainsKey(value);
        }

        public bool Add(object value)
        {
            if (hashtable.ContainsKey(value))
            {
                return false;
            }

            try
            {
                hashtable.Add(value, true);
                return true;
            }
            catch(ArgumentException)
            {
                // The item is already in the hashtable.
                return false;
            }
        }

        public void Remove(object value)
        {
            hashtable.Remove(value);
        }
    }
}
