using AutoMapper;
using EventManagement.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventManagement.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
            var users = await _unitOfWork.Admin.GetAllUsersExceptAdminsAsync();
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
            var pendingEvents = await _unitOfWork.Admin.GetPendingEventsAsync();
            return View(pendingEvents);
        }

        // GET: /Admin/EventApprovalDetails/5
        public async Task<IActionResult> EventApprovalDetails(int id)
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            var eventDetails = await _unitOfWork.Admin.GetEventByIdAsync(id);

            if (eventDetails == null || eventDetails.Status != "Pending")
            {
                return NotFound();
            }

            return View(eventDetails);
        }

        // POST: /Admin/ApproveEvent
        [HttpPost]
        public async Task<IActionResult> ApproveEvent(int eventId)
        {
            if (!IsAdmin()) return Unauthorized();

            await _unitOfWork.Admin.ApproveEventAsync(eventId);
            return RedirectToAction("EventApproval");
        }

        // POST: /Admin/RejectEvent
        [HttpPost]
        public async Task<IActionResult> RejectEvent(int eventId)
        {
            if (!IsAdmin()) return Unauthorized();

            await _unitOfWork.Admin.RejectEventAsync(eventId);
            return RedirectToAction("EventApproval");
        }

        public IActionResult MyRegistrations()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myRegs = _unitOfWork.Registrations.GetByUserIdAsync(userId.Value).Result;
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

            var myEvents = _unitOfWork.Events.GetByCreatorIdAsync(userId.Value).Result;
            return View(myEvents);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            var user = await _unitOfWork.Admin.GetUserByIdAsync(userId);
            if (user != null)
            {
                await _unitOfWork.Admin.DeleteUserAsync(userId);
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