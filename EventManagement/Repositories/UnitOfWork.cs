using EventManagement.Data;
using System.Threading.Tasks;
namespace EventManagement.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IUserRepository Users { get; }
        public IEventRepository Events { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Users = new UserRepository(context);
            Events = new EventRepository(context);
        }


        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }

}
