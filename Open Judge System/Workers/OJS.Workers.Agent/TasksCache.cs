namespace OJS.Workers.Agent
{
    using System;
    using System.IO;

    public class FileCache : IFileCache
    {
        private static FileCache instance;
        private readonly string cacheFilesPath = Environment.CurrentDirectory + @"\Tasks\";

        private FileCache()
        {
            Directory.CreateDirectory(this.cacheFilesPath);
        }

        public static FileCache Instance => instance ?? (instance = new FileCache());

        public byte[] this[string key]
        {
            get
            {
                return File.ReadAllBytes(this.GetCacheLocation(key));
            }

            set
            {
                File.WriteAllBytes(this.GetCacheLocation(key), value);
            }
        }

        public bool Contains(string key)
        {
            var result = File.Exists(this.GetCacheLocation(key));
            return result;
        }

        private string GetCacheLocation(string key)
        {
            return $"{this.cacheFilesPath}{key}.cache";
        }
    }
}
