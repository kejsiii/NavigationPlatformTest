using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class JourneyPublicLink
    {
        [Key]
        public Guid Id { get; set; }
        public string Token { get; set; }
        public Guid JourneyId { get; set; }
        [ForeignKey(nameof(JourneyId))]
        public Journey Journey { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }

    }
}
