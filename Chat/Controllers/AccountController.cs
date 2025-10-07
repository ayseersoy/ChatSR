using Chat.Models;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;

namespace Chat.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(UserLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            if (_db.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("", "Username already exists");
                return View(model);
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var user = new User
            {
                Username = model.Username,
                PasswordHash = passwordHash
            };

            _db.Users.Add(user);
            _db.SaveChanges();
            return RedirectToAction("Login");

        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(UserLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var user = _db.Users.FirstOrDefault(u => u.Username == model.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }
            // Set session or cookie
            HttpContext.Session.SetString("Username", user.Username);
            return RedirectToAction("Index", "Chat");
        }
    }
}
