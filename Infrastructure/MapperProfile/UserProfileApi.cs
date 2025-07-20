using AutoMapper;
using DTO.DTO.User;
using DTO.WebApiDTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.MapperProfile
{
    public class UserProfileApi : Profile
    {
        public UserProfileApi() 
        {
            CreateMap<RegisterRequestDto, RegisterRequestDto>();
            CreateMap<RegisterResponseDto, RegisterResponseDto>();
            CreateMap<DTO.WebApiDTO.User.LoginRequestDtoApi, DTO.WebApiDTO.User.LoginRequestDtoApi>();

        }
    }
}
