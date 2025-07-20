using AutoMapper;
using Domain.Entities;
using DTO.DTO.Journey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Application.MapperProfile
{
    public class JourneyProfile : Profile
    {
        public JourneyProfile() 
        {
            CreateMap<Journey, JourneyDto>();
            CreateMap<AddJourneyRequestDto, Journey>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.JourneyShares, opt => opt.Ignore())
                .ForMember(dest => dest.JourneyPublicLinks, opt => opt.Ignore());

            CreateMap<JourneyPublicLink, JourneyPublicLinkDto>();
        }
    }
}
