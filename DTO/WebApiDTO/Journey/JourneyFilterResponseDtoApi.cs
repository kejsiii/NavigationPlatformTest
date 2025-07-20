using DTO.DTO.Journey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.WebApiDTO.Journey
{
    public class JourneyFilterResponseDtoApi
    {
        public int TotalCount { get; set; }
        public List<JourneyDto> Items { get; set; }
    }
}
