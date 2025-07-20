using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class JourneyShare
    {
        [Key]
        public Guid Id { get; set; }

        // The journey that is being shared
        public Guid JourneyId { get; set; }
        [ForeignKey(nameof(JourneyId))]
        public Journey Journey { get; set; }

        // User who shares the journey
        public Guid SharedByUserId { get; set; }
        [ForeignKey(nameof(SharedByUserId))]
        public User SharingUser { get; set; }

        // User who receives the journey
        public Guid RecievingUserId { get; set; }
        [ForeignKey(nameof(RecievingUserId))]
        public User RecievingUser { get; set; }

        public DateTime SharedAt { get; set; }
        public bool IsRevoked { get; set; }

    }
}
