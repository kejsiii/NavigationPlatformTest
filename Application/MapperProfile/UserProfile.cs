using AutoMapper;
using Domain.Entities;
using DTO.DTO.User;

namespace Application.MapperProfile
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<RegisterRequestDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RoleId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.Journeys, opt => opt.Ignore())
                .ForMember(dest => dest.DailyGoalBadges, opt => opt.Ignore())
                .ForMember(dest => dest.SharedJourneys, opt => opt.Ignore())
                .ForMember(dest => dest.ReceivedJourneys, opt => opt.Ignore())
                .ForMember(dest => dest.AuditLog, opt => opt.Ignore())
                .ForMember(dest => dest.StatusChanges, opt => opt.Ignore())
                .ForMember(dest => dest.PerformedStatusChanges, opt => opt.Ignore());
        }
    }

}
