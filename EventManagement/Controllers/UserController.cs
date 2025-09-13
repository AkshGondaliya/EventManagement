using EventManagement.Data;
using EventManagement.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EventManagement.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /User/Profile
        public async Task<IActionResult> Profile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: /User/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return NotFound();

            var viewModel = new ProfileViewModel
            {
                FullName = user.FullName,
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return View(viewModel);
        }

        // ADD THIS ACTION to save the updated profile
        // POST: /User/EditProfile
        [HttpPost]
        public async Task<IActionResult> EditProfile(ProfileViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return NotFound();

            user.FullName = viewModel.FullName;
            user.ProfilePictureUrl = viewModel.ProfilePictureUrl;

            _context.Update(user);
            await _context.SaveChangesAsync();

            // Update the name in the session
            HttpContext.Session.SetString("FullName", user.FullName);

            return RedirectToAction("Profile");
        }

    }
}
