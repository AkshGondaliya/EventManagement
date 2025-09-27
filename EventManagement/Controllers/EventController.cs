using EventManagement.Data;
using EventManagement.Documents;
using EventManagement.Models;
using EventManagement.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using QRCoder;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagement.Controllers
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

            if (!String.IsNullOrEmpty(searchString))
            {
                eventsQuery = eventsQuery.Where(e => e.Title.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(category))
            {
                eventsQuery = eventsQuery.Where(e => e.Category == category);
            }

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

        // POST: /Event/Create
        [HttpPost]
        public async Task<IActionResult> Create(Event ev)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ev.EventDateTime < DateTime.Now.AddHours(24))
            {
                ModelState.AddModelError("EventDateTime", "The event date and time must be at least 24 hours from now.");
            }

            if (!ModelState.IsValid)
            {
                return View(ev);
            }

            ev.CreatedBy = userId.Value;
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

            if (await _context.Registrations.AnyAsync(r => r.EventId == id && r.UserId == userId.Value))
            {
                TempData["Notification"] = "You are already registered for this event.";
                return RedirectToAction("Details", new { id });
            }

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

            var eventDetails = await _context.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.EventId == viewModel.EventId);

            if (eventDetails.Registrations.Any(r => r.UserId == userId.Value))
            {
                TempData["Notification"] = "You are already registered for this event.";
                return RedirectToAction("Details", new { id = viewModel.EventId });
            }

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
                Status = "Registered",
                Semester = viewModel.Semester,
                Branch = viewModel.Branch
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            TempData["SuccessNotification"] = "You have successfully registered for the event!";
            return RedirectToAction("MyRegistrations");
        }

        // GET: /Event/ViewTicket/5
        public async Task<IActionResult> ViewTicket(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var registration = await _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.RegistrationId == id);

            if (registration == null || registration.UserId != userId)
            {
                return NotFound();
            }

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode($"RegID:{registration.RegistrationId}", QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeImageBytes = qrCode.GetGraphic(20);

            // Convert the byte array to a Base64 string
            string qrCodeImageUrl = "data:image/png;base64," + Convert.ToBase64String(qrCodeImageBytes);

            var viewModel = new ViewTicketViewModel
            {
                RegistrationId = registration.RegistrationId,
                EventTitle = registration.Event.Title,
                AttendeeName = registration.User.FullName,
                EventDate = registration.Event.EventDateTime,
                Venue = registration.Event.Venue,
                QrCodeImageUrl = qrCodeImageUrl
            };

            return View(viewModel);
        }

        // GET: /Event/DownloadTicket/5
        public async Task<IActionResult> DownloadTicket(int id)
        {
            string ticketUrl = Url.Action("ViewTicket", "Event", new { id }, Request.Scheme);
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();
            await page.GoToAsync(ticketUrl, WaitUntilNavigation.Networkidle0);

            var pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Format = PuppeteerSharp.Media.PaperFormat.A4,
                Landscape = true
            });

            return File(pdfBytes, "application/pdf", $"EventTicket_{id}.pdf");
        }

        // POST: /Event/CancelRegistration
        [HttpPost]
        public async Task<IActionResult> CancelRegistration(int registrationId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Forbid();
            }

            var registration = await _context.Registrations.FindAsync(registrationId);

            if (registration == null || registration.UserId != userId)
            {
                return Forbid();
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