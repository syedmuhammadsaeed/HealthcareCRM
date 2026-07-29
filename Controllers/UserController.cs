using Microsoft.AspNetCore.Mvc;
using HealthcareCRM.Data;
using HealthcareCRM.Models;
using System.Security.Claims;
using MongoDB.Driver;
using System.Linq;

using Microsoft.AspNetCore.Authorization;

namespace HealthcareCRM.Controllers
{
    [Authorize(Roles = "User,Admin,SuperAdmin")]
    public class UserController : Controller
    {
        private readonly MongoDbContext _context;

        public UserController(MongoDbContext context)
        {
            _context = context;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        private void SetLayoutData()
        {
            var userId = GetUserId();
            var user = _context.Users.Find(u => u.Id == userId).FirstOrDefault();
            ViewBag.UserName = user?.Name ?? "User";
            ViewBag.UserProfilePic = user?.ProfilePictureUrl;
        }

        public IActionResult Index()
        {
            SetLayoutData();
            var userId = GetUserId();
            var appointments = _context.Appointments.Find(a => a.UserId == userId).ToList();
            var labReports = _context.LabReports.Find(l => l.UserId == userId).ToList();
            var nextAppointment = appointments.Where(a => a.ScheduledAt > System.DateTime.UtcNow && a.Status == "Approved").OrderBy(a => a.ScheduledAt).FirstOrDefault();
            
            ViewBag.UserPatientId = "P-" + (userId.Length >= 6 ? userId.Substring(userId.Length - 6).ToUpper() : "123456");
            ViewBag.UpcomingCount = appointments.Count(a => a.ScheduledAt > System.DateTime.UtcNow && a.Status == "Approved");
            ViewBag.PendingLabCount = labReports.Count(l => l.Status == "Pending");
            ViewBag.CompletedLabCount = labReports.Count(l => l.Status == "Completed");
            ViewBag.TodayAppointmentsCount = appointments.Count(a => a.ScheduledAt.Date == System.DateTime.UtcNow.Date);
            ViewBag.NextAppointment = nextAppointment;
            ViewBag.Appointments = appointments.OrderByDescending(a => a.ScheduledAt).Take(5).ToList();
            
            return View();
        }

        public IActionResult BookAppointment()
        {
            SetLayoutData();
            var userId = GetUserId();
            var user = _context.Users.Find(u => u.Id == userId).FirstOrDefault();
            
            if (user == null || string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Phone) || string.IsNullOrEmpty(user.Gender) || string.IsNullOrEmpty(user.Address) || string.IsNullOrEmpty(user.EmergencyContactName) || string.IsNullOrEmpty(user.EmergencyContactNumber))
            {
                TempData["ErrorMessage"] = "Please complete your profile details before booking an appointment.";
                return RedirectToAction("Settings");
            }

            var doctors = _context.Users.Find(u => u.Role == "Doctor" && u.Status == "Approved").ToList();
            return View(doctors);
        }

        [HttpPost]
        public IActionResult BookAppointment(string doctorId, string specialty, System.DateTime scheduledAt)
        {
            var doctor = _context.Users.Find(u => u.Id == doctorId && u.Role == "Doctor").FirstOrDefault();
            if (doctor == null) return BadRequest();

            var appointment = new Appointment
            {
                UserId = GetUserId(),
                DoctorId = doctor.Id,
                DoctorName = doctor.Name,
                Specialty = specialty,
                ScheduledAt = scheduledAt,
                Status = "Pending",
                Fee = doctor.Fee,
                Currency = doctor.Currency
            };

            _context.Appointments.InsertOne(appointment);
            return RedirectToAction("AppointmentHistory");
        }

        public IActionResult AppointmentHistory(string searchTerm = "", string statusTab = "All", int page = 1, int pageSize = 10)
        {
            SetLayoutData();
            var userId = GetUserId();
            var query = _context.Appointments.Find(a => a.UserId == userId).ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(a => (a.DoctorName != null && a.DoctorName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                                         (a.Specialty != null && a.Specialty.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            if (statusTab == "Upcoming")
                query = query.Where(a => a.ScheduledAt > DateTime.UtcNow && (a.Status == "Approved" || a.Status == "Pending")).ToList();
            else if (statusTab == "Completed")
                query = query.Where(a => a.Status == "Completed").ToList();
            else if (statusTab == "Cancelled")
                query = query.Where(a => a.Status == "Cancelled" || a.Status == "Rejected").ToList();

            query = query.OrderByDescending(a => a.ScheduledAt).ToList();

            int totalItems = query.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            var pagedData = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.StatusTab = statusTab;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;

            return View(pagedData);
        }

        public IActionResult LabReports(string searchTerm = "", string status = "")
        {
            SetLayoutData();
            var userId = GetUserId();
            var query = _context.LabReports.Find(r => r.UserId == userId).ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r => r.ReportName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(r => r.Status == status).ToList();
            }

            var reports = query.OrderByDescending(r => r.DateOrdered).ToList();
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            return View(reports);
        }

        [HttpGet]
        public IActionResult PreviewReport(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return NotFound("Invalid file URL");
            
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", fileUrl.TrimStart('/'));
            if (!System.IO.File.Exists(path))
                return NotFound("File not found or has been deleted.");

            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            Response.Headers.Add("Content-Disposition", "inline; filename=" + Path.GetFileName(path));
            return File(stream, "application/pdf");
        }

        public IActionResult Settings()
        {
            SetLayoutData();
            var userId = GetUserId();
            var user = _context.Users.Find(u => u.Id == userId).FirstOrDefault();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Settings(string name, DateTime dateOfBirth, string gender, string phone, string address, string emergencyContactName, string emergencyContactRelationship, string emergencyContactNumber, IFormFile? profilePhoto)
        {
            var userId = GetUserId();
            var user = _context.Users.Find(u => u.Id == userId).FirstOrDefault();
            if (user != null)
            {
                user.Name = name;
                user.DateOfBirth = dateOfBirth;
                user.Gender = gender;
                user.Phone = phone;
                user.Address = address;
                user.EmergencyContactName = emergencyContactName;
                user.EmergencyContactRelationship = emergencyContactRelationship;
                user.EmergencyContactNumber = emergencyContactNumber;
                
                if (profilePhoto != null && profilePhoto.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profilePhoto.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePhoto.CopyToAsync(stream);
                    }
                    user.ProfilePictureUrl = "/uploads/" + fileName;
                }
                
                _context.Users.ReplaceOne(u => u.Id == userId, user);
                TempData["SuccessMessage"] = "Profile updated successfully.";
            }
            return RedirectToAction("Settings");
        }
        [HttpPost]
        public IActionResult UpdatePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = GetUserId();
            var user = _context.Users.Find(u => u.Id == userId).FirstOrDefault();

            if (user == null) return RedirectToAction("Settings");

            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "New passwords do not match.";
                return RedirectToAction("Settings");
            }

            // Verification against existing hash
            var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);

            if (result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
            {
                TempData["ErrorMessage"] = "Current password is incorrect.";
                return RedirectToAction("Settings");
            }

            user.PasswordHash = passwordHasher.HashPassword(user, newPassword);
            _context.Users.ReplaceOne(u => u.Id == userId, user);

            TempData["SuccessMessage"] = "Password updated successfully.";
            return RedirectToAction("Settings");
        }
    }
}
