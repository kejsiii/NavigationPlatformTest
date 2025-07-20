using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUserRepository : IRepositoryBase<User>
    {
        Task<User> FindByEmailOrUsernameAsync(string email, string username);
        Task<User> GetByEmailAsync(string email, string password);
    }
}
