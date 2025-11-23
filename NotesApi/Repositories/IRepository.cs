public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> CreateAsync(T T);
    Task<T?> UpdateAsync(T T);
    Task<bool> DeleteAsync(int id);
}