using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class JourneyPublicLinkRepository : IJourneyPublicLinkRepository
    {
        private AppDbContext _context;
        public JourneyPublicLinkRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Guid> AddAsync(JourneyPublicLink journeyPublicLink)
        {
            await _context.JourneyPublicLinks.AddAsync(journeyPublicLink);
            await _context.SaveChangesAsync();
            return journeyPublicLink.Id;
        }

        public async Task<List<JourneyPublicLink>> GetAll()
        {
            return await _context.JourneyPublicLinks.ToListAsync();
        }

        public async Task<JourneyPublicLink> GetAsyncById(Guid id)
        {
            return await _context.JourneyPublicLinks.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task RemoveAsync(JourneyPublicLink journeyPublicLink)
        {
            _context.JourneyPublicLinks.Remove(journeyPublicLink);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(JourneyPublicLink journeyPublicLink)
        {
            _context.JourneyPublicLinks.Update(journeyPublicLink);
            await _context.SaveChangesAsync();
        }

        public async void SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAll(List<JourneyPublicLink> journeyPublicLinks)
        {
            _context.JourneyPublicLinks.RemoveRange(journeyPublicLinks);
            await _context.SaveChangesAsync();
        }

        public async Task<JourneyPublicLink> GetJourneyPublicLinkRevokedByJourneyId(Guid journeyId)
        {
            return await _context.JourneyPublicLinks.FirstOrDefaultAsync(l => l.JourneyId == journeyId && !l.IsRevoked);
        }

        public async Task<JourneyPublicLink> GetPublicLinkByToken(string token)
        {
            return await _context.JourneyPublicLinks.FirstOrDefaultAsync(x => x.Token.ToString().Equals(token));
        }
    }
}
