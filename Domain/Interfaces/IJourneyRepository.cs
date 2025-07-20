using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IJourneyRepository : IRepositoryBase<Journey>
    {
        Task<Journey> GetJourneyByUserIdAndStartTime(Guid userId, DateTime dateTime);
        IQueryable<Journey> GetJourneysByUserId(Guid userId);
        Task<List<Journey>> GetJourneysForDateAsync(DateTime date);
        IQueryable<Journey> GetAllJournies();
    }
}
