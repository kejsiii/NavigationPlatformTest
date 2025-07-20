using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IJourneyShareRepository : IRepositoryBase<JourneyShare>
    {
        Task DeleteAll(List<JourneyShare> journeyShares);
    }
}
