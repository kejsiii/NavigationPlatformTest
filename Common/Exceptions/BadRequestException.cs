using System.Net;

namespace Common.Exceptions
{
    public class BadRequestException : BaseException
    {
        public BadRequestException(string details) : base(details)
        {
            Type = HttpCodeTypes.Error400Type;
            Title = HttpStatusCode.BadRequest.ToString();
            Status = (int)HttpStatusCode.BadRequest;
        }
    }
}
