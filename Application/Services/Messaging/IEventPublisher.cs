using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Messaging
{
    public interface IEventPublisher
    {
        Task PublishAsync(string eventName, object payload, CancellationToken cancellationToken = default);
    }
}
