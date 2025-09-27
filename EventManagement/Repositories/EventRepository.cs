using EventManagement.Data;
using EventManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagement.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;

        public EventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Event> GetByIdAsync(int id)
        {
            return await _context.Events
                .Include(e => e.Creator)
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.EventId == id);
        }

        public async Task<IEnumerable<Event>> GetAllApprovedAsync()
        {
            return await _context.Events
                .Include(e => e.Registrations)
                .Where(e => e.Status == "Approved")
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetPendingApprovalAsync()
        {
            return await _context.Events
                .Include(e => e.Creator)
                .Where(e => e.Status == "Pending")
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetByCreatorIdAsync(int userId)
        {
            return await _context.Events
                .Where(e => e.CreatedBy == userId && e.Status == "Approved")
                .ToListAsync();
        }

        public async Task AddAsync(Event entity)
        {
            await _context.Events.AddAsync(entity);
        }

        public void Update(Event entity)
        {
            _context.Events.Update(entity);
        }

        public void Delete(Event entity)
        {
            _context.Events.Remove(entity);
        }
    }
}