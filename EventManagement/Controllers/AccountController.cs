using AutoMapper;
using BCrypt.Net;
using EventManagement.Models;
using EventManagement.Repositories;
using EventManagement.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Register(UserViewModel registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            // Check if email already exists
            if (_unitOfWork.Users.EmailExists(registerVM.Email))
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

            _unitOfWork.Users.Add(user);
            _unitOfWork.Save();

            return RedirectToAction("Login");
        }

        // GET: Login
        public IActionResult Login() => View();

        // POST: Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _unitOfWork.Users.GetByEmail(email);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("FullName", user.FullName);

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
