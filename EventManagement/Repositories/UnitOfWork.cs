using EventManagement.Data;
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

        public void Save()
        {
            _context.SaveChanges();
        }
    }

}
