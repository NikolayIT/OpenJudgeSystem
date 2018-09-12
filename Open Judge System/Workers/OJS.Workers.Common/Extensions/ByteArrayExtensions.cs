namespace OJS.Workers.Common.Extensions
{
    using System;

    public static class ByteArrayExtensions
    {
        public static string ToHexString(this byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            var hexChars = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

            var bytesCount = bytes.Length;
            var resultChars = new char[(bytesCount * 2) + 2];

            resultChars[0] = '0';
            resultChars[1] = 'x';

            var bytesIndex = 0;
            var resultCharsIndex = 2;
            while (bytesIndex < bytesCount)
            {
                var @byte = bytes[bytesIndex++];
                resultChars[resultCharsIndex++] = hexChars[@byte / 0x10];
                resultChars[resultCharsIndex++] = hexChars[@byte % 0x10];
            }

            return new string(resultChars, 0, resultChars.Length);
        }
    }
}