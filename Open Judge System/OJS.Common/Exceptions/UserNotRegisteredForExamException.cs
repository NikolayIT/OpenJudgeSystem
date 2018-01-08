namespace OJS.Common.Exceptions
{
    using System.Web;

    public class UserNotRegisteredForExamException : HttpException
    {
        public UserNotRegisteredForExamException(string message)
            : base(message)
        {
        }

        public UserNotRegisteredForExamException(int statusCode, string message)
            : base(statusCode, message)
        {
        }
    }
}