using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Common.Exceptions
{
    public class GoneException :BaseException
    {
        public GoneException(string details) : base(details)
        {
            Type = HttpCodeTypes.Error410Type;
            Title = HttpStatusCode.Conflict.ToString();
            Status = (int)HttpStatusCode.Gone;
        }
    }
}
