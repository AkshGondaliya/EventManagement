using EventManagement.Data;
using EventManagement.Models;
using EventManagement.ViewModels; // Add this using statement
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CollegeEventManagement.Controllers // Note: Your namespace might be different
{
    public class EventController : Controller
    {
        private readonly AppDbContext _context;

        public EventController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Event/Index
        public IActionResult Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;

            var eventsQuery = _context.Events
                                      .Include(e => e.Registrations)
                                      .Where(e => e.Status == "Approved");

            if (!String.IsNullOrEmpty(searchString))
            {
                eventsQuery = eventsQuery.Where(e => e.Title.Contains(searchString)
                                               || e.Description.Contains(searchString));
            }

            var eventsList = eventsQuery.ToList();
            return View(eventsList);
        }

        // GET: /Event/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var eventDetails = await _context.Events
                .Include(e => e.Creator)
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (eventDetails == null) return NotFound();

            return View(eventDetails);
        }

        // GET: /Event/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // POST: Event/Create
        [HttpPost]
        public async Task<IActionResult> Create(Event ev)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Validate Event DateTime
            var now = DateTime.Now;
            if (ev.EventDateTime < now.AddHours(24))
            {
                ModelState.AddModelError("EventDateTime", "The event date and time must be at least 24 hours from now.");
            }

            if (!ModelState.IsValid)
            {
                // If validation fails, return the same view with the entered data
                return View(ev);
            }

            // Assign the creator
            ev.CreatedBy = userId.Value;

            // Set status based on role
            var userRole = HttpContext.Session.GetString("Role");
            if (userRole == "Admin")
            {
                ev.Status = "Approved";
                TempData["SuccessNotification"] = "Event created and approved successfully!";
            }
            else
            {
                ev.Status = "Pending";
                TempData["SuccessNotification"] = "Your event request has been submitted for approval!";
            }

            // Save to database
            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // GET: /Event/Register/5
        public async Task<IActionResult> Register(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FindAsync(userId.Value);
            var ev = await _context.Events.FindAsync(id);

            if (user == null || ev == null) return NotFound();

            var viewModel = new RegistrationViewModel
            {
                EventId = ev.EventId,
                EventTitle = ev.Title,
                EventFees = ev.Fees,
                UserId = user.UserId,
                UserFullName = user.FullName,
                UserEmail = user.Email
            };

            return View(viewModel);
        }

        // POST: /Event/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegistrationViewModel viewModel)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId.Value != viewModel.UserId)
            {
                return Forbid();
            }

            // Get event details with registrations
            var eventDetails = await _context.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.EventId == viewModel.EventId);

            if (eventDetails == null)
            {
                TempData["Notification"] = "The event does not exist.";
                return RedirectToAction("Index", "Home");
            }

            // 1. Check if already registered
            bool isAlreadyRegistered = eventDetails.Registrations.Any(r => r.UserId == userId.Value);
            if (isAlreadyRegistered)
            {
                TempData["Notification"] = "You are already registered for this event.";
                return RedirectToAction("Details", new { id = viewModel.EventId });
            }

            // 2. Check if the event is full
            if (eventDetails.Registrations.Count >= eventDetails.MaxParticipants)
            {
                TempData["Notification"] = "Registration is closed because this event is full.";
                return RedirectToAction("Details", new { id = viewModel.EventId });
            }

            // 3. Register the user
            var registration = new Registration
            {
                UserId = userId.Value,
                EventId = viewModel.EventId,
                RegistrationDate = DateTime.Now,
                Status = "Paid" // or "Pending" based on payment flow
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            TempData["SuccessNotification"] = "You have successfully registered for the event!";
            return RedirectToAction("MyRegistrations");
        }


        [HttpPost]
        public async Task<IActionResult> CancelRegistration(int registrationId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Forbid(); // User not logged in
            }

            // Find the registration to be deleted
            var registration = await _context.Registrations.FindAsync(registrationId);

            // Security check: ensure the registration belongs to the current user
            if (registration == null || registration.UserId != userId)
            {
                return Forbid(); // Or return a NotFound()
            }

            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();

            TempData["SuccessNotification"] = "You have successfully canceled your registration.";
            return RedirectToAction("MyRegistrations");
        }

        // GET: /Event/MyRegistrations
        public async Task<IActionResult> MyRegistrations()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myRegs = await _context.Registrations
                .Where(r => r.UserId == userId)
                .Include(r => r.Event)
                .ToListAsync();

            return View(myRegs);
        }

        // GET: /Event/MyCreatedEvents
        public async Task<IActionResult> MyCreatedEvents()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myEvents = await _context.Events
                .Where(e => e.CreatedBy == userId && e.Status == "Approved")
                .ToListAsync();

            return View(myEvents);
        }

        // GET: /Event/ViewRegistrations/5
        public async Task<IActionResult> ViewRegistrations(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var eventWithRegistrations = await _context.Events
                .Include(e => e.Registrations)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventWithRegistrations == null) return NotFound();
            if (eventWithRegistrations.CreatedBy != userId) return Forbid();

            return View(eventWithRegistrations);
        }
    }
}