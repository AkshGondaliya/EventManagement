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
            var eventsList = _context.Events.ToList();
            return View(eventsList);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(Event ev)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            ev.CreatedBy = userId;
            ev.Status = "Pending";

            _context.Events.Add(ev);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RegisterForEvent(int eventId)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Check if already registered
            var existing = _context.Registrations
                .SingleOrDefault(r => r.EventId == eventId && r.UserId == userId);

            if (existing != null)
            {
                ViewBag.Error = "You are already registered for this event!";
                return RedirectToAction("Index");
            }

            var registration = new Registration
            {
                UserId = userId,
                EventId = eventId,
                RegistrationDate = DateTime.Now,
                Status = "Registered"
            };

            _context.Registrations.Add(registration);
            _context.SaveChanges();

            return RedirectToAction("MyRegistrations");
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
