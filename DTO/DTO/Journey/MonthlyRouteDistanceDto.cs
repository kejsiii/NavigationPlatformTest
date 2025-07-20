using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.DTO.Journey
{
    public class MonthlyRouteDistanceDto
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string? OrderBy { get; set; }
        public Guid? UserId { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
    }
}
