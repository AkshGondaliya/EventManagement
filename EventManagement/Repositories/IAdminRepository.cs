using EventManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManagement.Repositories
{
    public interface IAdminRepository
    {
        Task<IEnumerable<User>> GetAllUsersExceptAdminsAsync();
        Task<User> GetUserByIdAsync(int userId);
        Task DeleteUserAsync(int userId);
        Task<IEnumerable<Event>> GetPendingEventsAsync();
        Task<Event> GetEventByIdAsync(int eventId);
        Task ApproveEventAsync(int eventId);
        Task RejectEventAsync(int eventId);
    }
}
