using System.Net;

namespace Common.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string details) : base(details)
        {
            Type = HttpCodeTypes.Error404Type;
            Title = HttpStatusCode.NotFound.ToString();
            Status = (int)HttpStatusCode.NotFound;
        }
    }
}
