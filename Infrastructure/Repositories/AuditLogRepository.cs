using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private AppDbContext _context;
        public AuditLogRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Guid> AddAsync(AuditLog auditLog)
        {
            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            return auditLog.Id;
        }


        public async Task<List<AuditLog>> GetAll()
        {
            return await _context.AuditLogs.ToListAsync();
        }

        public async Task<AuditLog> GetAsyncById(Guid Id)
        {
            return await _context.AuditLogs.FirstOrDefaultAsync(x => x.Id == Id);
        }

        public async Task RemoveAsync(AuditLog auditLog)
        {
            _context.AuditLogs.Remove(auditLog);
            await _context.SaveChangesAsync();
        }

        public async void SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AuditLog auditLog)
        {
            _context.Update(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}
