using EventManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManagement.Repositories
{
    public interface IEventRepository
    {
        Task<Event> GetByIdAsync(int id);
        Task<IEnumerable<Event>> GetAllApprovedAsync();
        Task<IEnumerable<Event>> GetPendingApprovalAsync();
        Task<IEnumerable<Event>> GetByCreatorIdAsync(int userId);
        Task AddAsync(Event entity);
        void Update(Event entity);
        void Delete(Event entity);
    }
}