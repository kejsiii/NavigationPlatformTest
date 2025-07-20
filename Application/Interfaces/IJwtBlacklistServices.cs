using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Interfaces
{
    public interface IJwtBlacklistServices
    {
        Task AddToBlacklistAsync(string token);
        Task<bool> IsBlacklistedAsync(string token);
    }


}