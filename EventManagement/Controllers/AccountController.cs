using AutoMapper;
using BCrypt.Net;
using EventManagement.Models;
using EventManagement.Repositories;
using EventManagement.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EventManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AccountController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: Register
        public IActionResult Register() => View();

        // POST: Register
        [HttpPost]
        public async Task<IActionResult> Register(UserViewModel registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            // Check if email already exists
            if (await _unitOfWork.Users.EmailExistsAsync(registerVM.Email))
            {
                ViewBag.Error = "Email already exists!";
                return View(registerVM);
            }

            // Map ViewModel to Entity
            var user = _mapper.Map<User>(registerVM);

            // Hash password before saving
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerVM.Password);

            // Assign default role
            user.Role = "Student";

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveAsync();

            return RedirectToAction("Login");
        }

        // GET: Login
        public IActionResult Login() => View();

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("FullName", user.FullName);
                // ADD THIS LINE
                HttpContext.Session.SetString("ProfilePictureUrl", user.ProfilePictureUrl ?? "");

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid login attempt!";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
