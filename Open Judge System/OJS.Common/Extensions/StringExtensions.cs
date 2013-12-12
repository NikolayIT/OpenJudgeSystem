namespace OJS.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class StringExtensions
    {
        public static byte[] ToByteArray(this string sourceString)
        {
            var encoding = new UTF8Encoding();
            return encoding.GetBytes(sourceString);
        }

        public static string ToText(this byte[] bytes)
        {
            var encoding = new UTF8Encoding();
            return encoding.GetString(bytes);
        }

        public static string ToUrl(this string uglyString)
        {
            var resultString = new StringBuilder(uglyString.Length);
            bool isLastCharacterDash = false;

            uglyString = uglyString.Replace("C#", "CSharp");
            uglyString = uglyString.Replace("C++", "CPlusPlus");

            foreach (var character in uglyString)
            {
                if (char.IsLetterOrDigit(character))
                {
                    resultString.Append(character);
                    isLastCharacterDash = false;
                }
                else if (!isLastCharacterDash)
                {
                    resultString.Append('-');
                    isLastCharacterDash = true;
                }
            }

            return resultString.ToString().Trim('-');
        }

        public static string Repeat(this string input, int count)
        {
            var builder = new StringBuilder((input == null ? 0 : input.Length) * count);

            for (int i = 0; i < count; i++)
            {
                builder.Append(input);
            }

            return builder.ToString();
        }

        public static string GetFileExtension(this string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return string.Empty;
            }

            string[] fileParts = fileName.Split(new[] { "." }, StringSplitOptions.None);
            if (fileParts.Count() == 1 || string.IsNullOrEmpty(fileParts.Last()))
            {
                return string.Empty;
            }

            return fileParts.Last().Trim().ToLower();
        }

        public static string MaxLength(this string stringToTrim, int maxLength)
        {
            if (stringToTrim == null || stringToTrim.Length <= maxLength)
            {
                return stringToTrim;
            }

            return stringToTrim.Substring(0, maxLength);
        }

        // TODO: Unit test
        public static string MaxLengthWithEllipsis(this string stringToTrim, int maxLength)
        {
            if (stringToTrim == null || stringToTrim.Length <= maxLength)
            {
                return stringToTrim;
            }

            return stringToTrim.Substring(0, maxLength) + "...";
        }

        // TODO: Test
        public static IEnumerable<string> GetStringsBetween(this string stringToParse, string beforeString, string afterString)
        {
            var regEx = new Regex(Regex.Escape(beforeString) + "(.*?)" + Regex.Escape(afterString), RegexOptions.Singleline | RegexOptions.Compiled);
            var matches = regEx.Matches(stringToParse);
            foreach (Match match in matches)
            {
                yield return match.Groups[1].Value;
            }
        }

        // TODO: Test
        public static string GetStringBetween(this string stringToParse, string beforeString, string afterString)
        {
            var strings = stringToParse.GetStringsBetween(beforeString, afterString).ToList();
            if (!strings.Any())
            {
                return null;
            }

            return strings[0];
        }

        // TODO: Test
        public static SecureString ToSecureString(this string sourceString)
        {
            var secureString = new SecureString();
            foreach (var character in sourceString)
            {
                secureString.AppendChar(character);
            }

            return secureString;
        }

        // TODO: Test
        public static string ToUrlSafeString(this string input)
        {
            input = input.Replace("+", "Plus");
            input = input.Replace("#", "Sharp");
            return input;
        }

        // TODO: Test
        public static string FromUrlSafeString(this string input)
        {
            input = input.Replace("Plus", "+");
            input = input.Replace("Sharp", "#");
            return input;
        }
    }
}
