using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class UserStatusChange
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }
        public User User { get; set; }

        public string OldStatus { get; set; }
        public string NewStatus { get; set; }

        // The admin who performed the change
        [ForeignKey(nameof(ChangedByAdmin))]
        public Guid ChangedByAdminId { get; set; }
        public User ChangedByAdmin { get; set; }

        public DateTime ChangedAt { get; set; }
    }
}
