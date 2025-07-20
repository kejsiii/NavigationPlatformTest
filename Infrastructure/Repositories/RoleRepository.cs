using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private AppDbContext _context;
        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Guid> AddAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
            return role.Id;
        }

        public async Task<List<Role>> GetAll()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Role> GetAsyncById(Guid roleId)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId);
        }

        public async Task RemoveAsync(Role role)
        {
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Role role)
        {
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
        }
        public async void SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Role> GetRoleByName(string name)
        {
            return await _context.Roles.FirstOrDefaultAsync(x => x.Name == name);
        }
    }
}
