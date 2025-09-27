using EventManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManagement.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);

        Task AddAsync(User user);
        void Update(User user);
        void Delete(User user);

        Task<bool> EmailExistsAsync(string email);

        Task<IEnumerable<User>> GetAllUsersExceptAdminsAsync();
    }
    
}
