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
        public async Task<IActionResult> Index(string searchString, string sortOrder, string category)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentCategory"] = category;

            var eventsQuery = _context.Events
                                      .Include(e => e.Registrations)
                                      .Where(e => e.Status == "Approved");

            // Filtering Logic
            if (!String.IsNullOrEmpty(searchString))
            {
                eventsQuery = eventsQuery.Where(e => e.Title.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(category))
            {
                eventsQuery = eventsQuery.Where(e => e.Category == category);
            }

            // Sorting Logic
            switch (sortOrder)
            {
                case "name_desc":
                    eventsQuery = eventsQuery.OrderByDescending(e => e.Title);
                    break;
                case "Date":
                    eventsQuery = eventsQuery.OrderBy(e => e.EventDateTime);
                    break;
                case "date_desc":
                    eventsQuery = eventsQuery.OrderByDescending(e => e.EventDateTime);
                    break;
                default:
                    eventsQuery = eventsQuery.OrderBy(e => e.Title);
                    break;
            }

            // Get a list of unique category names for the filter buttons
            ViewData["Categories"] = await _context.Events
                                                .Where(e => e.Status == "Approved")
                                                .Select(e => e.Category)
                                                .Distinct()
                                                .ToListAsync();

            var eventsList = await eventsQuery.ToListAsync();
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
                UserEmail = user.Email,
                UserCollegeName = user.CollegeName
            };

            return View(viewModel);
        }

        // POST: /Event/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegistrationViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate details if validation fails
                var ev = await _context.Events.FindAsync(viewModel.EventId);
                var user = await _context.Users.FindAsync(viewModel.UserId);
                viewModel.EventTitle = ev.Title;
                viewModel.UserFullName = user.FullName;
                viewModel.UserEmail = user.Email;
                viewModel.UserCollegeName = user.CollegeName;
                return View(viewModel);
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId.Value != viewModel.UserId)
            {
                return Forbid();
            }

            // Efficiently get event and registration count in one query
            var eventDetails = await _context.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.EventId == viewModel.EventId);

            // 1. Check if already registered
            if (eventDetails.Registrations.Any(r => r.UserId == userId.Value))
            {
                TempData["Notification"] = "You are already registered for this event.";
                return RedirectToAction("Details", new { id = viewModel.EventId });
            }

            // 2. Check if the event is full
            if (eventDetails.Registrations.Count >= eventDetails.MaxParticipants)
            {
                TempData["Notification"] = "Registration failed because this event is now full.";
                return RedirectToAction("Details", new { id = viewModel.EventId });
            }

            var registration = new Registration
            {
                UserId = viewModel.UserId,
                EventId = viewModel.EventId,
                RegistrationDate = DateTime.Now,
                Status = "Paid",
                Semester = viewModel.Semester,
                Branch = viewModel.Branch
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
                .Where(e => e.CreatedBy == userId)
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