using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Messaging
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; }
        public string ExchangeName { get; set; }
    }
}
