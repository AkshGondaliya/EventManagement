using EventManagement.Data;
using EventManagement.Data;
using EventManagement.Models;
using EventManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace CollegeEventManagement.Controllers
{
    public class EventController : Controller
    {
        private readonly AppDbContext _context;

        public EventController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Only show events that have been approved
            var eventsList = _context.Events.Where(e => e.Status == "Approved").ToList();
            return View(eventsList);
        }

        // GET: Event/Create
        public IActionResult Create()
        {
            var userRole = HttpContext.Session.GetString("Role");
            // Only Admins or Event-Coordinators can create events
            if (userRole != "Admin" && userRole != "Event-Coordinator")
            {
                // Set the message that the layout file will display
                TempData["Notification"] = "You do not have permission to create an event.";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Event/Create
        [HttpPost]
        public IActionResult Create(Event ev)
        {
            var userRole = HttpContext.Session.GetString("Role");
            if (userRole != "Admin" && userRole != "Event-Coordinator")
            {
                return RedirectToAction("Index", "Home");
            }

            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            ev.CreatedBy = userId;
            ev.Status = "Pending"; // New events are pending approval

            _context.Events.Add(ev);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RegisterForEvent(int eventId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            // If userId is null, the user is not logged in.
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if already registered
            var existing = _context.Registrations
                .SingleOrDefault(r => r.EventId == eventId && r.UserId == userId);

            if (existing != null)
            {
                TempData["Error"] = "You are already registered for this event!";
                return RedirectToAction("Index");
            }

            var registration = new Registration
            {
                UserId = userId.Value,
                EventId = eventId,
                RegistrationDate = DateTime.Now,
                Status = "Registered"
            };

            _context.Registrations.Add(registration);
            _context.SaveChanges();

            TempData["Success"] = "Successfully registered for the event!";
            return RedirectToAction("Index");
        }

        public IActionResult MyRegistrations()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var myRegs = _context.Registrations
                .Where(r => r.UserId == userId)
                .Select(r => new
                {
                    r.RegistrationId,
                    r.Event.Title,
                    r.RegistrationDate,
                    r.Status
                }).ToList();

            return View(myRegs);
        }
    }
    }
