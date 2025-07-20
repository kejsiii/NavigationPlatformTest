using System.Net;

namespace Common.Exceptions
{
    public class ForbiddenException : BaseException
    {
        public ForbiddenException(string details) : base(details)
        {
            Type = HttpCodeTypes.Error403Type;
            Title = HttpStatusCode.Forbidden.ToString();
            Status = (int)HttpStatusCode.Forbidden;
        }
    }
}
