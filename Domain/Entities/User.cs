using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Status { get; set; }
        public Guid RoleId { get; set; }
        [ForeignKey(nameof(RoleId))]
        public Role Role { get; set; }

        public virtual ICollection<Journey> Journeys { get; set; }
        public virtual ICollection<DailyGoalBadge> DailyGoalBadges { get; set; }
        public virtual ICollection<JourneyShare> SharedJourneys { get; set; }
        public virtual ICollection<JourneyShare> ReceivedJourneys { get; set; }
        public virtual ICollection<AuditLog> AuditLog { get; set; }
        public virtual ICollection<UserStatusChange> StatusChanges { get; set; }
        public virtual ICollection<UserStatusChange> PerformedStatusChanges { get; set; }

    }
}
