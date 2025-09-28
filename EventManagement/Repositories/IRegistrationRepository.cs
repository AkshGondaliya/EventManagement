using EventManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManagement.Repositories
{
    public interface IRegistrationRepository
    {
        Task<Registration> GetByIdAsync(int id);
        Task<IEnumerable<Registration>> GetByUserIdAsync(int userId);
        Task<bool> ExistsAsync(int eventId, int userId);
        Task AddAsync(Registration entity);
        void Delete(Registration entity);
    }
}