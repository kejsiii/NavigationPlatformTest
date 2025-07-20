using Domain.Entities;
using DTO.DTO.Journey;

namespace Application.Interfaces
{
    public interface IJourneyServices
    {
        Task<Guid> AddJourneyAsync(AddJourneyRequestDto request);
        Task<JourneyDto> GetJourneyByIdAsync(Guid journeyId);
        Task<List<JourneyDto>> GetAllJourneysForUserAsync(Guid userId);
        Task DeleteJourneyAsync(Guid journeyId);
        Task<JourneyShareResponseDto> ShareJourneyAsync(Guid journeyId,Guid userId,JourneyShareRequestDto request);
        Task<PublicJourneyLinkResponseDto> GeneratePublicLinkAsync(Guid id, Guid userId);
        Task RevokePublicLinkAsync(Guid journeyId, Guid userId);
        Task<JourneyPublicLinkDto> GetPublicJourneyByTokenAsync(string token);
        Task<JourneyFilterResponseDto> GetJourniesByFilter(JourneyFilterRequestDto request);
        Task<List<MonthlyRouteDistanceResponseDto>> GetMonthlyDistancesAsync(MonthlyRouteDistanceDto filter);

    }
}

