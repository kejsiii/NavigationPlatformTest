using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.WebApiDTO.Journey
{
    public class JourneyDtoApi
    {
        public Guid JourneyId { get; set; }
        public string StartingLocation { get; set; }
        public string ArrivalLocation { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string TransportationType { get; set; }
        public decimal RouteDistanceKm { get; set; }
        public bool IsDailyGoalAchieved { get; set; }
        public Guid UserId { get; set; }
    }
}
