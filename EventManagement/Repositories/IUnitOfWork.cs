using System.Threading.Tasks;

namespace EventManagement.Repositories
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IEventRepository Events { get; }
        IRegistrationRepository Registrations { get; }
        INotificationRepository Notifications { get; }
        Task SaveAsync();
    }

}
