using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IRoleRepository : IRepositoryBase<Role>
    {
        Task<Role> GetRoleByName(string name);
    }
}
