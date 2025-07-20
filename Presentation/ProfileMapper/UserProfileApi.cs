using AutoMapper;
using DTO.DTO.User;
using DTO.WebApiDTO.User;

namespace Presentation.ProfileMapper
{
    public class UserProfileApi : Profile
    {
        public UserProfileApi()
        {
            CreateMap<RegisterRequestDtoApi, RegisterRequestDto>();
            CreateMap<RegisterResponseDto, RegisterResponseDtoApi>();
            CreateMap<LoginRequestDtoApi, LoginRequestDto>();
        }
    }
}
