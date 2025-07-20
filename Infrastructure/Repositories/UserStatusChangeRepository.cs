using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserStatusChangeRepository : IUserStatusChangeRepository
    {
        private AppDbContext _context;
        public UserStatusChangeRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Guid> AddAsync(UserStatusChange userStatusChange)
        {
            await _context.UserStatusChanges.AddAsync(userStatusChange);
            await _context.SaveChangesAsync();
            return userStatusChange.Id;
        }

        public async Task<List<UserStatusChange>> GetAll()
        {
            return await _context.UserStatusChanges.ToListAsync();
        }

        public async Task<UserStatusChange> GetAsyncById(Guid userStatusChangeId)
        {
            return await _context.UserStatusChanges
                .FirstOrDefaultAsync(x => x.Id == userStatusChangeId);
        }

        public async Task RemoveAsync(UserStatusChange userStatusChange)
        {
            _context.UserStatusChanges.Remove(userStatusChange);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserStatusChange userStatusChange)
        {
            _context.UserStatusChanges.Update(userStatusChange);
            await _context.SaveChangesAsync();
        }

        public async void SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
