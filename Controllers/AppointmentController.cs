using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HealthcareCRM.Data;
using HealthcareCRM.Models;
using MongoDB.Driver;
using System.Linq;

namespace HealthcareCRM.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly MongoDbContext _context;

        public AppointmentController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "Admin" && role != "SuperAdmin")
            {
                return RedirectToAction("Index", "Home");
            }
            
            var appointments = _context.Appointments.Find(a => true).ToList().OrderByDescending(a => a.ScheduledAt).ToList();
            var users = _context.Users.Find(u => true).ToList();
            
            foreach (var appt in appointments)
            {
                var patient = users.FirstOrDefault(u => u.Id == appt.UserId);
                appt.PatientName = patient?.Name ?? "Unknown";
                appt.PatientPhone = patient?.Phone ?? "Unknown";
            }
            return View(appointments);
        }

        [HttpPost]
        public IActionResult Approve(string id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "Admin" && role != "SuperAdmin")
            {
                return Unauthorized();
            }

            var update = Builders<Appointment>.Update.Set(a => a.Status, "Approved");
            _context.Appointments.UpdateOne(a => a.Id == id, update);

            var appt = _context.Appointments.Find(a => a.Id == id).FirstOrDefault();
            if (appt != null)
            {
                var user = _context.Users.Find(u => u.Id == appt.UserId).FirstOrDefault();
                if (user != null)
                {
                    var existing = _context.Patients.Find(p => p.UserId == user.Id).FirstOrDefault();
                    if (existing == null)
                    {
                        var patient = new Patient
                        {
                            Name = user.Name,
                            DateOfBirth = user.DateOfBirth ?? DateTime.UtcNow,
                            Gender = user.Gender ?? "Unknown",
                            Phone = user.Phone ?? "Unknown",
                            Address = user.Address ?? "Unknown",
                            Status = "active",
                            AssignedDoctorId = appt.DoctorId,
                            AppointmentDate = appt.ScheduledAt.Date,
                            AppointmentTime = appt.ScheduledAt.ToString("h:mm tt"),
                            AppointmentStatus = "Approved",
                            AppointmentFee = appt.Fee,
                            AppointmentCurrency = appt.Currency,
                            IsOnline = true,
                            UserId = user.Id,
                            CreatedDate = DateTime.UtcNow
                        };
                        _context.Patients.InsertOne(patient);
                    }
                    else
                    {
                        var updatePatient = Builders<Patient>.Update
                            .Set(p => p.AssignedDoctorId, appt.DoctorId)
                            .Set(p => p.AppointmentDate, appt.ScheduledAt.Date)
                            .Set(p => p.AppointmentTime, appt.ScheduledAt.ToString("h:mm tt"))
                            .Set(p => p.AppointmentStatus, "Approved")
                            .Set(p => p.AppointmentFee, appt.Fee)
                            .Set(p => p.AppointmentCurrency, appt.Currency)
                            .Set(p => p.IsOnline, true);
                        _context.Patients.UpdateOne(p => p.Id == existing.Id, updatePatient);
                    }
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Cancel(string id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "Admin" && role != "SuperAdmin")
            {
                return Unauthorized();
            }

            var update = Builders<Appointment>.Update.Set(a => a.Status, "Cancelled");
            _context.Appointments.UpdateOne(a => a.Id == id, update);

            return RedirectToAction("Index");
        }
    }
}
