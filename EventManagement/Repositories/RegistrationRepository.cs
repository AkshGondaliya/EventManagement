using EventManagement.Data;
using EventManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagement.Repositories
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly AppDbContext _context;

        public RegistrationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Registration> GetByIdAsync(int id)
        {
            return await _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.RegistrationId == id);
        }

        public async Task<IEnumerable<Registration>> GetByUserIdAsync(int userId)
        {
            return await _context.Registrations
                .Where(r => r.UserId == userId)
                .Include(r => r.Event)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int eventId, int userId)
        {
            return await _context.Registrations.AnyAsync(r => r.EventId == eventId && r.UserId == userId);
        }

        public async Task AddAsync(Registration entity)
        {
            await _context.Registrations.AddAsync(entity);
        }

        public void Delete(Registration entity)
        {
            _context.Registrations.Remove(entity);
        }
    }
}