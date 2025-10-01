using EventManagement.Data;
using EventManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagement.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly AppDbContext _context;
        public AdminRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<User>> GetAllUsersExceptAdminsAsync()
        {
            return await _context.Users.Where(u => u.Role != "Admin").ToListAsync();
        }
        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }
        public async Task DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Event>> GetPendingEventsAsync()
        {
            return await _context.Events.Include(e => e.Creator).Where(e => e.Status == "Pending").ToListAsync();
        }
        public async Task<Event> GetEventByIdAsync(int eventId)
        {
            return await _context.Events.Include(e => e.Creator).FirstOrDefaultAsync(e => e.EventId == eventId);
        }
        public async Task ApproveEventAsync(int eventId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev != null)
            {
                ev.Status = "Approved";
                await _context.SaveChangesAsync();
            }
        }
        public async Task RejectEventAsync(int eventId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev != null)
            {
                ev.Status = "Rejected";
                await _context.SaveChangesAsync();
            }
        }
    }
}
