using HealthcareCRM.Interfaces;
using HealthcareCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareCRM.Controllers
{
    [Authorize]
    public class SuperAdminController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly Helpers.PasswordHasher _passwordHasher;

        public SuperAdminController(IUserRepository userRepository, Helpers.PasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<IActionResult> Index()
        {
            var pendingUsers = await _userRepository.GetAllPendingAsync();
            return View(pendingUsers);
        }

        public IActionResult Profile()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ApproveUser(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user != null && user.Status == "Pending")
            {
                user.Status = "Approved";
                await _userRepository.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RejectUser(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user != null && user.Status == "Pending")
            {
                user.Status = "Rejected";
                await _userRepository.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateStaff(string role, string name, string email, string password, string gender, int? age, string specialization)
        {
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
            {
                TempData["Error"] = "Email is already registered.";
                return RedirectToAction("Index");
            }

            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = _passwordHasher.HashPassword(password),
                CreatedDate = DateTime.UtcNow,
                Role = role,
                Status = "Approved",
                Gender = gender,
                Age = age,
                Specialization = role == "Doctor" ? specialization : null
            };

            await _userRepository.AddAsync(user);
            TempData["Success"] = $"{role} account created successfully.";
            return RedirectToAction("Index");
        }
    }
}
