using AutoMapper;
using DTO.DTO.Journey;
using DTO.WebApiDTO.Journey;
using DTO.WebApiDTO.User;

namespace Presentation.ProfileMapper
{
    public class JourneyProfileApi : Profile
    {
        public JourneyProfileApi() 
        {
            CreateMap<AddJourneyRequestDtoApi, AddJourneyRequestDto>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

            CreateMap<JourneyDto, JourneyDtoApi>();

            CreateMap<JourneyShareRequestDtoApi, JourneyShareRequestDto>();
            CreateMap<JourneyShareResponseDto, JourneyShareResponseDtoApi>();
            CreateMap<PublicJourneyLinkResponseDto, PublicJourneyLinkResponseDtoApi>();
            CreateMap<JourneyPublicLinkDto,JourneyPublicLinkDtoApi>();
            CreateMap<JourneyFilterRequestDtoApi, JourneyFilterRequestDto>();
            CreateMap<JourneyFilterResponseDto, JourneyFilterResponseDtoApi>();
            CreateMap<MonthlyRouteDistanceDtoApi, MonthlyRouteDistanceDto>();
            CreateMap<MonthlyRouteDistanceResponseDto, MonthlyRouteDistanceResponseDtoApi>();
        }
    }
}
