using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Utilities
{
    public class JwtUser
    {
        public JwtUser()
        {
        }

        public string Role { get; set; }
        public object Id { get; set; }
        public object Password { get; set; }
        public string Email { get; set; }
    }
}
