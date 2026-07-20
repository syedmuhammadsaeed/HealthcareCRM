using HealthcareCRM.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HealthcareCRM.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly IUserRepository _userRepository;

        public SettingsController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return RedirectToAction("Login", "Account");

            return View(user);
        }

        public class ProfileUpdateDto
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string ProfilePictureBase64 { get; set; } = string.Empty;
        }

        [HttpPost]
        [Route("api/settings/profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized(new { success = false, message = "User not found." });

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return Unauthorized(new { success = false, message = "User not found." });

            user.Name = model.Name;
            user.Email = model.Email;
            
            if (!string.IsNullOrEmpty(model.ProfilePictureBase64))
            {
                user.ProfilePictureUrl = model.ProfilePictureBase64;
            }
            
            await _userRepository.UpdateAsync(user);

            return Ok(new { success = true, message = "Profile updated.", url = user.ProfilePictureUrl });
        }

        [HttpPost]
        [Route("api/settings/upload-avatar")]
        public async Task<IActionResult> UploadAvatar(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { success = false, message = "No file uploaded." });
            if (file.Length > 2 * 1024 * 1024) return BadRequest(new { success = false, message = "File size exceeds 2MB." });
            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized(new { success = false, message = "User not found." });

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return Unauthorized(new { success = false, message = "User not found." });

            var uploadsPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            if (!System.IO.Directory.Exists(uploadsPath)) System.IO.Directory.CreateDirectory(uploadsPath);

            var ext = System.IO.Path.GetExtension(file.FileName);
            var newFileName = $"{userId}_{System.DateTime.UtcNow.Ticks}{ext}";
            var filePath = System.IO.Path.Combine(uploadsPath, newFileName);

            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.ProfilePictureUrl = $"/uploads/avatars/{newFileName}";
            await _userRepository.UpdateAsync(user);

            return Ok(new { success = true, url = user.ProfilePictureUrl });
        }
    }
}
