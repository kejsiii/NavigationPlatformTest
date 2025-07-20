using System.Net;

namespace Common.Exceptions
{
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string details) : base(details)
        {
            Type = HttpCodeTypes.Error401Type;
            Title = HttpStatusCode.Unauthorized.ToString();
            Status = (int)HttpStatusCode.Unauthorized;
        }
    }
}
