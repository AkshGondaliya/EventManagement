using EventManagement.Models;
using System.Threading.Tasks;

namespace EventManagement.Repositories
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
    }
}