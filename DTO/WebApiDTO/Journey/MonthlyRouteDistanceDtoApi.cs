using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.WebApiDTO.Journey
{
    public class MonthlyRouteDistanceDtoApi
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        [SwaggerSchema("Allowed values: 'UserId' or 'TotalDistanceKm'")]
        public string? OrderBy { get; set; }
        public Guid? UserId { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
    }
 
}
