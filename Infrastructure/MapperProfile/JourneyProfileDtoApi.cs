using AutoMapper;
using DTO.DTO.Journey;
using DTO.WebApiDTO.Journey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.MapperProfile
{
    public class JourneyProfileDtoApi : Profile
    {
        JourneyProfileDtoApi() 
        {
            CreateMap<JourneyDto, JourneyDtoApi>();
        }
    }
}
