using System.Net;

namespace Common.Exceptions
{
    public class ConflictException : BaseException
    {
        public ConflictException(string details) : base(details)
        {
            Type = HttpCodeTypes.Error409Type;
            Title = HttpStatusCode.Conflict.ToString();
            Status = (int)HttpStatusCode.Conflict;
        }
    }
}
