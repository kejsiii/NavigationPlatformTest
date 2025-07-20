using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IJourneyPublicLinkRepository : IRepositoryBase<JourneyPublicLink>
    {
        Task DeleteAll(List<JourneyPublicLink> journeyPublicLinks);
        Task<JourneyPublicLink> GetJourneyPublicLinkRevokedByJourneyId(Guid journeyId);
        Task <JourneyPublicLink> GetPublicLinkByToken(string token);
    }
}
