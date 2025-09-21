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

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                // Important: You might need to handle related data first,
                // like deleting registrations associated with this user.
                // For now, we will assume the database handles it or there are none.
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessNotification"] = "User successfully deleted.";
            }
            else
            {
                TempData["Notification"] = "User not found.";
            }

            return RedirectToAction("UserManagement");
        }
    }
}