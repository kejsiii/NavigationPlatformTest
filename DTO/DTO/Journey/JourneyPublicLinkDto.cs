using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.DTO.Journey
{
    public class JourneyPublicLinkDto
    {
        public Guid Id { get; set; }
        public string Token { get; set; }
        public Guid JourneyId { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}
