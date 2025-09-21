using EventManagement.Data;
using EventManagement.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EventManagement.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; // for profileImage Upload

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
                ProfilePictureUrl = user.ProfilePictureUrl,
                CollegeName = user.CollegeName
            };

            return View(viewModel);
        }

        // ADD THIS ACTION to save the updated profile
        [HttpPost]
        public async Task<IActionResult> EditProfile(ProfileViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            var user = await _context.Users.FindAsync(userId.Value);

            if (viewModel.ProfileImage != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/avatars");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.ProfileImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.ProfileImage.CopyToAsync(fileStream);
                }
                user.ProfilePictureUrl = "/uploads/avatars/" + uniqueFileName;
                HttpContext.Session.SetString("ProfilePictureUrl", user.ProfilePictureUrl);
            }

            user.FullName = viewModel.FullName;
            user.CollegeName = viewModel.CollegeName;

            _context.Update(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("FullName", user.FullName);

            return RedirectToAction("Profile");
        }


        // GET: /User/ChangePassword
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /User/ChangePassword
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel viewModel)
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
            if (user == null)
            {
                return NotFound();
            }

            // 1. Verify the old password is correct
            if (!BCrypt.Net.BCrypt.Verify(viewModel.OldPassword, user.PasswordHash))
            {
                ModelState.AddModelError("OldPassword", "The current password is incorrect.");
                return View(viewModel);
            }

            // 2. Hash and save the new password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(viewModel.NewPassword);
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessNotification"] = "Your password has been changed successfully.";
            return RedirectToAction("Profile");
        }


    }
}
