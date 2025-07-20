using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Guid> AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user.Id;
        }

        public async Task<User> FindByEmailOrUsernameAsync(string email, string username)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Username == username && x.Email != email);
        }

        public async Task<List<User>> GetAll()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetAsyncById(Guid Id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == Id);
        }

        public async Task RemoveAsync(User user)
        {
            _context.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async void SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetByEmailAsync(string email, string password)
        {
            return await _context.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Email == email && x.Password == password);
        }
    }
}
