using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class JourneyRepository : IJourneyRepository
    {
        private AppDbContext _context;
        public JourneyRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Guid> AddAsync(Journey journey)
        {
            await _context.Journeys.AddAsync(journey);
            await _context.SaveChangesAsync();
            return journey.JourneyId;
        }

        public async Task<List<Journey>> GetAll()
        {
            return await _context.Journeys.ToListAsync();
        }

        public async Task<Journey> GetAsyncById(Guid id)
        {
            return await _context.Journeys.FirstOrDefaultAsync(x => x.JourneyId == id);
        }

        public async Task RemoveAsync(Journey journey)
        {
            _context.Journeys.Remove(journey);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Journey journey)
        {
            _context.Journeys.Update(journey);
            await _context.SaveChangesAsync();
        }

        public async void SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Journey> GetJourneyByUserIdAndStartTime(Guid userId, DateTime dateTime)
        {
            return await _context.Journeys.FirstOrDefaultAsync(x => (x.UserId == userId && x.StartTime == dateTime)
             || (x.UserId == userId && x.StartTime.Date == dateTime.Date && x.ArrivalTime.Hour > dateTime.Hour));
        }

        public IQueryable<Journey> GetJourneysByUserId(Guid userId)
        {
            return _context.Journeys.Where(x => x.UserId == userId);
        }

        public async Task<List<Journey>> GetJourneysForDateAsync(DateTime date)
        {
            return await _context.Journeys.Where(x => x.StartTime.Date == date.Date).ToListAsync();
        }

        public IQueryable<Journey> GetAllJournies()
        {
            return _context.Journeys;
        }
    }
}
