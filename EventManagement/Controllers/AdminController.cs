using EventManagement.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagement.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            // Simple role check based on the session
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // GET: /Admin/UserManagement
        public async Task<IActionResult> UserManagement()
        {
            if (!IsAdmin())
            {
                // If not an admin, redirect them away
                return RedirectToAction("Index", "Home");
            }

            // Fetch all users except the currently logged-in Admin
            var users = await _context.Users.Where(u => u.Role != "Admin").ToListAsync();
            return View(users);
        }

        // POST: /Admin/PromoteToCoordinator
        [HttpPost]
        public async Task<IActionResult> PromoteToCoordinator(int userId)
        {
            if (!IsAdmin()) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user != null && user.Role == "Student")
            {
                user.Role = "Event-Coordinator";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("UserManagement");
        }

        // POST: /Admin/DemoteToStudent
        [HttpPost]
        public async Task<IActionResult> DemoteToStudent(int userId)
        {
            if (!IsAdmin()) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user != null && user.Role == "Event-Coordinator")
            {
                user.Role = "Student";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("UserManagement");
        }

        // GET: /Admin/EventApproval
        public async Task<IActionResult> EventApproval()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            // Fetch all events that are pending approval
            var pendingEvents = await _context.Events
                                              .Where(e => e.Status == "Pending")
                                              .Include(e => e.Creator) // Include creator's info
                                              .ToListAsync();
            return View(pendingEvents);
        }

        // POST: /Admin/ApproveEvent
        [HttpPost]
        public async Task<IActionResult> ApproveEvent(int eventId)
        {
            if (!IsAdmin()) return Unauthorized();

            var ev = await _context.Events.FindAsync(eventId);
            if (ev != null)
            {
                ev.Status = "Approved";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("EventApproval");
        }

        // POST: /Admin/RejectEvent
        [HttpPost]
        public async Task<IActionResult> RejectEvent(int eventId)
        {
            if (!IsAdmin()) return Unauthorized();

            var ev = await _context.Events.FindAsync(eventId);
            if (ev != null)
            {
                ev.Status = "Rejected";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("EventApproval");
        }

        public IActionResult MyRegistrations()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myRegs = _context.Registrations
                .Where(r => r.UserId == userId)
                .Include(r => r.Event) // Include Event details
                .ToList();

            return View(myRegs);
        }

        public IActionResult MyCreatedEvents()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("Role");

            if (userId == null || (userRole != "Admin" && userRole != "Event-Coordinator"))
            {
                return RedirectToAction("Index", "Home");
            }

            var myEvents = _context.Events
                .Where(e => e.CreatedBy == userId)
                .ToList();

            return View(myEvents);
        }
    }
}