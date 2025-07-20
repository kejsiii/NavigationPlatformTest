using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DTO.DTO.Journey
{
    public class JourneyDto
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
