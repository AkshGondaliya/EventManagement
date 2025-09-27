using EventManagement.Data;
using EventManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagement.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        // Get all users
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.AsNoTracking().ToListAsync();
        }

        // Get user by ID
        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        // Get user by email
        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        // Add new user
        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        // Update user
        public void Update(User user)
        {
            _context.Users.Update(user);
        }

        // Delete user
        public void Delete(User user)
        {
            _context.Users.Remove(user);
        }

        // Check if email exists
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        // Get all non-admin users
        public async Task<IEnumerable<User>> GetAllUsersExceptAdminsAsync()
        {
            return await _context.Users
                .Where(u => u.Role != "Admin")
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
