using EventManagement.Data;
using EventManagement.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EventManagement.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; // for profileImage Upload
        private readonly ILogger<UserController> _logger;


        public UserController(AppDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<UserController> logger)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
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
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound();
            }

            // Handle the file upload
            if (viewModel.ProfileImage != null)
            {
                // Store the path of the old image before updating it
                string oldImagePath = user.ProfilePictureUrl;

                // Save the new image
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/avatars");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.ProfileImage.FileName;
                string newFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                {
                    await viewModel.ProfileImage.CopyToAsync(fileStream);
                }

                // Update the user's record with the new path
                user.ProfilePictureUrl = "/uploads/avatars/" + uniqueFileName;

                // If an old image existed, delete the old file from the server
                if (!string.IsNullOrEmpty(oldImagePath))
                {
                    string oldAbsoluteImagePath = Path.Combine(_webHostEnvironment.WebRootPath, oldImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldAbsoluteImagePath))
                    {
                        System.IO.File.Delete(oldAbsoluteImagePath);
                    }
                }
            }

            // Update other user properties
            user.FullName = viewModel.FullName;
            user.CollegeName = viewModel.CollegeName;
            _context.Update(user);
            await _context.SaveChangesAsync();

            // Update session with new details
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("ProfilePictureUrl", user.ProfilePictureUrl ?? "");

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
