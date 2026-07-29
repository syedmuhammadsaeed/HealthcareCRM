using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HealthcareCRM.Data;
using HealthcareCRM.Models;
using MongoDB.Driver;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace HealthcareCRM.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminLabReportController : Controller
    {
        private readonly MongoDbContext _context;

        public AdminLabReportController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var reports = _context.LabReports.Find(r => true).ToList();
            return View(reports);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Only online patients should be selected
            var onlinePatients = _context.Patients.Find(p => p.IsOnline).ToList();
            return View(onlinePatients);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string patientId, string reportName, string status, IFormFile? reportFile)
        {
            var patient = _context.Patients.Find(p => p.Id == patientId && p.IsOnline).FirstOrDefault();
            if (patient == null) return BadRequest("Invalid patient");

            var report = new LabReport
            {
                UserId = patient.UserId ?? string.Empty,
                ReportName = reportName,
                Status = status,
                DateOrdered = DateTime.UtcNow,
                OrderDoctor = patient.AssignedDoctorId != null 
                    ? _context.Users.Find(u => u.Id == patient.AssignedDoctorId).FirstOrDefault()?.Name ?? "Unknown" 
                    : "Unknown"
            };

            if (reportFile != null && reportFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "reports");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(reportFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await reportFile.CopyToAsync(stream);
                }
                report.FileUrl = "/uploads/reports/" + fileName;
            }

            _context.LabReports.InsertOne(report);
            return RedirectToAction("Index");
        }
    }
}
