using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class JourneyShareRepository : IJourneyShareRepository
    {
        private AppDbContext _context;
        public JourneyShareRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Guid> AddAsync(JourneyShare journeyShare)
        {
            await _context.JourneyShares.AddAsync(journeyShare);
            await _context.SaveChangesAsync();
            return journeyShare.Id;
        }

        public async Task<List<JourneyShare>> GetAll()
        {
            return await _context.JourneyShares.ToListAsync();
        }

        public async Task<JourneyShare> GetAsyncById(Guid id)
        {
            return await _context.JourneyShares.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task RemoveAsync(JourneyShare journeyShare)
        {
            _context.JourneyShares.Remove(journeyShare);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(JourneyShare journeyShare)
        {
            _context.JourneyShares.Update(journeyShare);
            await _context.SaveChangesAsync();
        }

        public async void SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAll(List<JourneyShare> journeyShares)
        {
            _context.JourneyShares.RemoveRange(journeyShares);
            await _context.SaveChangesAsync();
        }
    }
}
