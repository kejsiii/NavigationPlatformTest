using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.DTO.Journey
{
    public class JourneyFilterResponseDto
    {
        public int TotalCount { get; set; }
        public List<JourneyDto> Items { get; set; }
    }
}
