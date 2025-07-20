using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Journey
    {
        [Key]
        public Guid JourneyId { get; set; }
        public string StartingLocation { get; set; }
        public string ArrivalLocation { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string TransportationType { get; set; }
        public decimal RouteDistanceKm { get; set; }
        public bool IsDailyGoalAchieved { get; set; }

        public User User { get; set; }
        [ForeignKey("UserId")]
        public Guid UserId { get; set; }
        public virtual ICollection<JourneyPublicLink> JourneyPublicLinks { get; set; }
        public virtual ICollection<JourneyShare> JourneyShares { get; set; }
    }
}
