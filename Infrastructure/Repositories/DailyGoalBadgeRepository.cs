using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class DailyGoalBadgeRepository : IDailyGoalBadgeRepository
    {
        private AppDbContext _context;
        public DailyGoalBadgeRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Guid> AddAsync(DailyGoalBadge dailyGoalBadge)
        {
            await _context.DailyGoalBadges.AddAsync(dailyGoalBadge);
            await _context.SaveChangesAsync();
            return dailyGoalBadge.Id;
        }

        public async Task<List<DailyGoalBadge>> GetAll()
        {
            return await _context.DailyGoalBadges.ToListAsync();
        }

        public async Task<DailyGoalBadge> GetAsyncById(Guid id)
        {
            return await _context.DailyGoalBadges.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task RemoveAsync(DailyGoalBadge dailyGoalBadge)
        {
            _context.DailyGoalBadges.Remove(dailyGoalBadge);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DailyGoalBadge dailyGoalBadge)
        {
            _context.DailyGoalBadges.Update(dailyGoalBadge);
            await _context.SaveChangesAsync();
        }

        public async void SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsForUserOnDate(Guid userId, DateTime date)
        {
            return await _context.DailyGoalBadges.AnyAsync(b => b.UserId == userId && b.Date.Date == date.Date);
        }
    }
}
