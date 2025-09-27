using EventManagement.Data;
using System.Threading.Tasks;
namespace EventManagement.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IUserRepository Users { get; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Users = new UserRepository(context);
        }


        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }

}
