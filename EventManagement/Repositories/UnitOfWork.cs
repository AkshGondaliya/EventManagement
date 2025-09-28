using EventManagement.Data;
using System.Threading.Tasks;
namespace EventManagement.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IUserRepository Users { get; }
        public IEventRepository Events { get; }
        public IRegistrationRepository Registrations { get; }
        public INotificationRepository Notifications { get; }
        public IAdminRepository Admin { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Users = new UserRepository(context);
            Events = new EventRepository(context);
            Registrations = new RegistrationRepository(context);
            Notifications = new NotificationRepository(context);
            Admin = new AdminRepository(context);
        }


        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }

}
