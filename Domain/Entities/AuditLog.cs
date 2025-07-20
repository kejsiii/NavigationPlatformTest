using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class AuditLog
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid TargetId { get; set; }

        public string ActionType { get; set; }

        public DateTime Timestamp { get; set; }

        public string Description { get; set; }
    }
}
