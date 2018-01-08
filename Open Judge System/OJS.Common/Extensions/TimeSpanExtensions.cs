namespace OJS.Common.Extensions
{
    using System;
    using System.Linq;

    public static class TimeSpanExtensions
    {
        public static string ToReadableString(this TimeSpan span)
        {
            var formatted = string.Format(
                "{0}{1}{2}{3}",
                span.Days > 0 ? $"{span.Days:0} day{(span.Days == 1 ? string.Empty : "s")}," : string.Empty,
                span.Hours > 0 ? $"{span.Hours:0} hour{(span.Hours == 1 ? string.Empty : "s")}," : string.Empty,
                span.Minutes > 0 ? $"{span.Minutes:0} minute{(span.Minutes == 1 ? string.Empty : "s")}," : string.Empty,
                span.Seconds > 0 ? $"{span.Seconds:0} second{(span.Seconds == 1 ? string.Empty : "s")}" : string.Empty);

            var timeParts = formatted.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (timeParts.Count > 1)
            {
                var lastPart = " and " + timeParts[timeParts.Count - 1];
                timeParts.RemoveAt(timeParts.Count - 1);
                formatted = string.Join(", ", timeParts) + lastPart;
            }
            else if (formatted.EndsWith(","))
            {
                formatted = formatted.Substring(0, formatted.Length - 1);
            }

            if (string.IsNullOrWhiteSpace(formatted))
            {
                formatted = "0 seconds";
            }

            return formatted;
        }
    }
}