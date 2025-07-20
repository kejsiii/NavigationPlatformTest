namespace Domain.Interfaces
{
    public interface IRepositoryBase<T> where T : class, new()
    {

        Task<T> GetAsyncById(Guid Id);
        Task<List<T>> GetAll();

        Task<Guid> AddAsync(T entity);

        Task UpdateAsync(T entity);

        Task RemoveAsync(T entity);
        void SaveChangesAsync();
    }
}
