namespace OJS.Web.Common.Exceptions
{
    using System.Web;

    public class ContestCannotBeCompetedException : HttpException
    {
        public ContestCannotBeCompetedException(int statusCode, string message)
            : base(statusCode, message)
        {
        }
    }
}