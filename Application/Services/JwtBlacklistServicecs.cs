using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
        public class JwtBlacklistService : IJwtBlacklistServices
        {
            private static readonly HashSet<string> _blacklist = new();

            public Task AddToBlacklistAsync(string token)
            {
                _blacklist.Add(token);
                return Task.CompletedTask;
            }

            public Task<bool> IsBlacklistedAsync(string token)
            {
                return Task.FromResult(_blacklist.Contains(token));
            }
        }
}

