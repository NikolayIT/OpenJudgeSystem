namespace OJS.Workers.Executors
{
    using System.Security;

    public static class StringExtensions
    {
        public static SecureString ToSecureString(this string sourceString)
        {
            var secureString = new SecureString();
            foreach (var character in sourceString)
            {
                secureString.AppendChar(character);
            }

            return secureString;
        }
    }
}
