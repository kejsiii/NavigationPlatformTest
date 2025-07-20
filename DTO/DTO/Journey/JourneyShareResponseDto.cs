using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.DTO.Journey
{
    public class JourneyShareResponseDto
    {
        public List<Guid> SharedWithUserIds { get; set; }
        public List<Guid> CreatedShareIds { get; set; }
    }
}
