using AutoMapper;
using EventManagement.Models;
using EventManagement.Repositories;
using EventManagement.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EventController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var events = await _unitOfWork.Events.GetAllApprovedAsync();
            if (!String.IsNullOrEmpty(searchString))
            {
                events = events.Where(e => e.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }
            return View(events);
        }

        public async Task<IActionResult> Details(int id)
        {
            var eventDetails = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventDetails == null)
            {
                return NotFound();
            }

            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            bool isRegistered = currentUserId.HasValue && await _unitOfWork.Registrations.ExistsAsync(eventDetails.EventId, currentUserId.Value);
           

            // Check if the logged-in user is the creator of the event
            bool isCreator = currentUserId.HasValue && eventDetails.CreatedBy == currentUserId.Value;
            ViewBag.IsCreator = isCreator;
            ViewBag.IsRegistered = isRegistered;
            return View(eventDetails);
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("UserId") == null) return RedirectToAction("Login", "Account");
            return View(new EventViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(EventViewModel viewModel)
        {
            // Ensure event is at least 24 hours in the future
            if (viewModel.EventDateTime <= DateTime.Now.AddHours(24))
            {
                ModelState.AddModelError("EventDateTime", "Event date and time must be at least 24 hours ahead from now.");
            }
            if (!ModelState.IsValid) return View(viewModel);

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var newEvent = _mapper.Map<Event>(viewModel);
            newEvent.CreatedBy = userId.Value;
            var userRole = HttpContext.Session.GetString("Role");

            newEvent.Status = (userRole == "Admin") ? "Approved" : "Pending";

            await _unitOfWork.Events.AddAsync(newEvent);
            await _unitOfWork.SaveAsync();

            if (userRole != "Admin")
            {
                TempData["RequestNotification"] = "Your request for Event is goes to the Admin";
            }
            return RedirectToAction("MyCreatedEvents");
        }

        public async Task<IActionResult> Edit(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var eventToEdit = await _unitOfWork.Events.GetByIdAsync(id);
            // Security check: ensure the user owns this event
            if (eventToEdit == null || eventToEdit.CreatedBy != userId) return Forbid();

            var viewModel = _mapper.Map<EventViewModel>(eventToEdit);
            return View(viewModel);
        }

        // POST: /Event/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(EventViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var eventToUpdate = await _unitOfWork.Events.GetByIdAsync(viewModel.EventId);
            // Security check: ensure the user owns this event
            if (eventToUpdate == null || eventToUpdate.CreatedBy != userId) return Forbid();

            _mapper.Map(viewModel, eventToUpdate); // Map updated fields to the existing event
            _unitOfWork.Events.Update(eventToUpdate);
            await _unitOfWork.SaveAsync();

            return RedirectToAction("MyCreatedEvents");
        }

        public async Task<IActionResult> Register(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            var ev = await _unitOfWork.Events.GetByIdAsync(id);

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

        [HttpPost]
        public async Task<IActionResult> Register(RegistrationViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || userId != viewModel.UserId) return Forbid();

            

            var registration = new Registration
            {
                UserId = viewModel.UserId,
                EventId = viewModel.EventId,
                RegistrationDate = DateTime.Now,
                Status = "Registered",
                Semester = viewModel.Semester,
                Branch = viewModel.Branch,
                PaymentMethod = viewModel.PaymentMethod
            };

            await _unitOfWork.Registrations.AddAsync(registration);
            await _unitOfWork.SaveAsync();

            TempData["SuccessNotification"] = "You have successfully registered for the event!";
            return RedirectToAction("MyRegistrations");
        }

        public async Task<IActionResult> MyRegistrations()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var myRegs = await _unitOfWork.Registrations.GetByUserIdAsync(userId.Value);
            return View(myRegs);
        }

        [HttpPost]
        public async Task<IActionResult> CancelRegistration(int registrationId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Forbid();

            var registration = await _unitOfWork.Registrations.GetByIdAsync(registrationId);
            if (registration == null ) return Forbid();

            _unitOfWork.Registrations.Delete(registration);
            await _unitOfWork.SaveAsync();

            TempData["SuccessNotification"] = "You have successfully canceled your registration.";
            return RedirectToAction("MyRegistrations");
        }
        public async Task<IActionResult> MyCreatedEvents()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var myEvents = await _unitOfWork.Events.GetByCreatorIdAsync(userId.Value);
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

            var eventWithRegistrations = await _unitOfWork.Events.GetByIdWithDetailsAsync(id);

            // Security check: ensure the user owns this event
            if (eventWithRegistrations == null )
            {
                return Forbid();
            }

            return View(eventWithRegistrations);
        }

       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete( int? eventId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Accept both 'id' and 'eventId' for compatibility with form
            int eventIdToDelete;
            //if (eventId.HasValue && eventId.Value != 0)
            //{
                eventIdToDelete = eventId.Value;
            //}

            var eventToDelete = await _unitOfWork.Events.GetByIdWithRegistrationsAsync(eventIdToDelete);

            
            // Create notifications for all participants using the repository
            foreach (var registration in eventToDelete.Registrations)
            {
                await _unitOfWork.Notifications.AddAsync(new Notification
                {
                    UserId = registration.UserId,
                    Message = $"The event '{eventToDelete.Title}' scheduled for {eventToDelete.EventDateTime:g} has been cancelled."
                });
            }

            // Delete the event using the repository
            _unitOfWork.Events.Delete(eventToDelete);

            // Save all changes (deletions and new notifications) in one transaction
            await _unitOfWork.SaveAsync();

            TempData["Notification"] = "Event deleted successfully and participants have been notified.";
            return RedirectToAction("MyCreatedEvents");
        }

        // GET: /Event/ViewTicket/5
        public async Task<IActionResult> ViewTicket(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var registration = await _unitOfWork.Registrations.GetByIdAsync(id);

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
        // In DownloadTicket action, before calling DownloadAsync, check if the browser is already downloaded.
        // Optionally, wrap DownloadAsync in a try-catch to handle file-in-use errors gracefully.

        public async Task<IActionResult> DownloadTicket(int id)
        {
            var browserFetcher = new BrowserFetcher();
            try
            {
                await browserFetcher.DownloadAsync();
            }
            catch (IOException)
            {
                // Ignore if already downloaded or in use
            }

            // Generate an authentication cookie for Puppeteer
            var authCookie = Request.Cookies[".AspNetCore.Session"];
            string ticketUrl = Url.Action("ViewTicket", "Event", new { id }, Request.Scheme);

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();

            // Set the session cookie so Puppeteer is authenticated
            if (!string.IsNullOrEmpty(authCookie))
            {
                await page.SetCookieAsync(new CookieParam
                {
                    Name = ".AspNetCore.Session",
                    Value = authCookie,
                    Domain = Request.Host.Host,
                    Path = "/"
                });
            }

            await page.GoToAsync(ticketUrl, WaitUntilNavigation.Networkidle0);
            await page.EvaluateExpressionAsync("document.querySelector('.d-print-none').remove();");
            var pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Format = PuppeteerSharp.Media.PaperFormat.A4,
                Landscape = true
            });

            return File(pdfBytes, "application/pdf", $"EventTicket_{id}.pdf");
        }
    }
}