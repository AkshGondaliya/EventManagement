using System.Threading.Tasks;

namespace EventManagement.Repositories
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        Task SaveAsync();
    }

}
