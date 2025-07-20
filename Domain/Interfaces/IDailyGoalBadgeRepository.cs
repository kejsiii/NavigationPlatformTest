using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IDailyGoalBadgeRepository : IRepositoryBase<DailyGoalBadge>
    {
        Task<bool> ExistsForUserOnDate(Guid userId, DateTime date);
    }
}
