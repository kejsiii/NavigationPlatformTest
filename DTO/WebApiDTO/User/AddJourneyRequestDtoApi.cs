using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.WebApiDTO.User
{
    public class AddJourneyRequestDtoApi
    {
        public string StartingLocation { get; set; }
        public string ArrivalLocation { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string TransportationType { get; set; }
        public decimal RouteDistanceKm { get; set; }
    }
}
