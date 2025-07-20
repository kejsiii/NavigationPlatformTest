using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.WebApiDTO.Journey
{
    public class MonthlyRouteDistanceResponseDtoApi
    {
        public Guid UserId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public double TotalDistanceKm { get; set; }
    }
}
