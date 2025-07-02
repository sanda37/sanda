using sanda.Models;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace sanda.Repositories
{
    public interface IServiceRepository
    {
        Task<IEnumerable<ServiceItem>> GetAllAsync();
        Task<ServiceItem> GetByIdAsync(int id);
        Task AddAsync(ServiceItem serviceItem);
        Task UpdateAsync(ServiceItem serviceItem);
        Task UpdateImageAsync(int id, string imageUrl);
        Task DeleteAsync(int id);
    }
}