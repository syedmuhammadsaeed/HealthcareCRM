using Microsoft.AspNetCore.Http;
using HealthcareCRM.Interfaces;
using HealthcareCRM.Models;
using HealthcareCRM.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MongoDB.Driver;
using System.Linq;

namespace HealthcareCRM.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly Helpers.PasswordHasher _passwordHasher;
        private readonly MongoDbContext _context;

        public SuperAdminController(IUserRepository userRepository, Helpers.PasswordHasher passwordHasher, MongoDbContext context)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _context = context;
        }

        private void SetLayoutData()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.Find(u => u.Id == userId).FirstOrDefault();
            ViewBag.UserName = user?.Name ?? "Super Admin";
            ViewBag.UserProfilePic = user?.ProfilePictureUrl;
            ViewBag.UserEmail = user?.Email;
            ViewBag.UserPhone = user?.Phone;
        }

        public async Task<IActionResult> Index()
        {
            SetLayoutData();
            var allUsers = await _context.Users.Find(_ => true).ToListAsync();
            ViewBag.TotalUsers = allUsers.Count(u => u.Role == "User");
            ViewBag.TotalDoctors = allUsers.Count(u => u.Role == "Doctor");
            ViewBag.TotalAdmins = allUsers.Count(u => u.Role == "Admin");
            return View();
        }

                public async Task<IActionResult> LoginHistory()
        {
            SetLayoutData();
            var users = await _context.Users.Find(u => u.Role == "Admin" || u.Role == "Doctor")
                .SortByDescending(u => u.LastLogin)
                .ToListAsync();
            return View(users);
        }

        
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string name, string email, string phone, IFormFile profilePicture, bool removePicture = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return NotFound();

            user.Name = name;
            user.Email = email;
            user.Phone = phone;

            if (removePicture)
            {
                user.ProfilePictureUrl = null;
            }
            else if (profilePicture != null && profilePicture.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + profilePicture.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(stream);
                }
                user.ProfilePictureUrl = "/uploads/" + uniqueFileName;
            }

            await _userRepository.UpdateAsync(user);
            return RedirectToAction("Profile");
        }

        public IActionResult Profile()
        {
            SetLayoutData();
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
        public async Task<IActionResult> CreateStaff(string role, string name, string email, string password, string gender, int? age, string specialization, string address, string phone)
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
                Specialization = role == "Doctor" ? specialization : null,
                Address = role == "Doctor" ? address : null,
                Phone = role == "Doctor" ? phone : null
            };

            await _userRepository.AddAsync(user);
            TempData["Success"] = $"{role} account created successfully.";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> ManageAccess()
        {
            SetLayoutData();
            var users = await _context.Users.Find(u => u.Role == "Admin" || u.Role == "Doctor").ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user != null)
            {
                if (user.Status == "Approved")
                {
                    user.Status = "Deactivated";
                }
                else if (user.Status == "Deactivated")
                {
                    user.Status = "Approved";
                }
                await _userRepository.UpdateAsync(user);
                TempData["Success"] = $"Account status updated to {user.Status}.";
            }
            return RedirectToAction("ManageAccess");
        }
    }
}
