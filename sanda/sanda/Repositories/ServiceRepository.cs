using Microsoft.EntityFrameworkCore;
using sanda.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using sanda.Models;
using sanda.Repositories;

namespace sanda.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly UserDbContext _context;

        public ServiceRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceItem>> GetAllAsync()
        {
            return await _context.ServiceItems.ToListAsync();
        }

        public async Task<ServiceItem> GetByIdAsync(int id)
        {
            return await _context.ServiceItems.FindAsync(id);
        }

        public async Task AddAsync(ServiceItem serviceItem)
        {
            _context.ServiceItems.Add(serviceItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ServiceItem serviceItem)
        {
            _context.ServiceItems.Update(serviceItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var serviceItem = await _context.ServiceItems.FindAsync(id);
            if (serviceItem != null)
            {
                _context.ServiceItems.Remove(serviceItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateImageAsync(int id, string imageUrl)
        {
            var serviceItem = await _context.ServiceItems.FindAsync(id);
            if (serviceItem != null)
            {
                serviceItem.Image = imageUrl;
                serviceItem.ImageLastUpdated = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

    }
}