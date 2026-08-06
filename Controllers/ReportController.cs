using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthcareCRM.Data;
using MongoDB.Driver;
using System.Linq;
using HealthcareCRM.Models;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using System.Text;

namespace HealthcareCRM.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class ReportController : Controller
    {
        private readonly MongoDbContext _context;

        public ReportController(MongoDbContext context)
        {
            _context = context;
        }

        private void SetLayoutData()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _context.Users.Find(u => u.Id == userId).FirstOrDefault();
            ViewBag.UserName = user?.Name ?? "Admin";
            ViewBag.UserProfilePic = user?.ProfilePictureUrl;
            ViewBag.UserRole = user?.Role;
        }

        public async Task<IActionResult> Index(string period = "year")
        {
            SetLayoutData();

            var patients = await _context.Patients.Find(_ => true).ToListAsync();
            var appointments = await _context.Appointments.Find(_ => true).ToListAsync();

            ViewBag.TotalPatientsCount = patients.Count;
            ViewBag.OnlinePatientsCount = patients.Count(p => p.IsOnline);
            ViewBag.TotalAppointmentsCount = appointments.Count;
            ViewBag.TotalRevenue = appointments.Sum(a => a.Fee ?? 0);

            ViewBag.Period = period;

            if (period == "month")
            {
                var currentYear = DateTime.UtcNow.Year;
                var currentMonth = DateTime.UtcNow.Month;
                var daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);

                var dailyPatients = new int[daysInMonth];
                var dailyOnlinePatients = new int[daysInMonth];
                var dailyRevenue = new decimal[daysInMonth];
                var dailyAppointments = new int[daysInMonth];

                foreach(var p in patients)
                {
                    if (p.CreatedDate.Year == currentYear && p.CreatedDate.Month == currentMonth)
                    {
                        var dayIndex = p.CreatedDate.Day - 1;
                        dailyPatients[dayIndex]++;
                        if (p.IsOnline)
                        {
                            dailyOnlinePatients[dayIndex]++;
                        }
                    }
                }

                foreach(var a in appointments)
                {
                    if (a.ScheduledAt.Year == currentYear && a.ScheduledAt.Month == currentMonth)
                    {
                        var dayIndex = a.ScheduledAt.Day - 1;
                        dailyAppointments[dayIndex]++;
                        dailyRevenue[dayIndex] += (a.Fee ?? 0);
                    }
                }

                ViewBag.ChartDataPatients = string.Join(",", dailyPatients);
                ViewBag.ChartDataOnlinePatients = string.Join(",", dailyOnlinePatients);
                ViewBag.ChartDataRevenue = string.Join(",", dailyRevenue);
                ViewBag.ChartDataAppointments = string.Join(",", dailyAppointments);
                
                var labels = new string[daysInMonth];
                for(int i = 0; i < daysInMonth; i++) labels[i] = (i + 1).ToString();
                ViewBag.ChartLabels = "'" + string.Join("','", labels) + "'";
                ViewBag.PeriodLabel = "This Month";
            }
            else
            {
                var currentYear = DateTime.UtcNow.Year;

                var monthlyPatients = new int[12];
                var monthlyOnlinePatients = new int[12];
                var monthlyRevenue = new decimal[12];
                var monthlyAppointments = new int[12];

                foreach(var p in patients)
                {
                    if (p.CreatedDate.Year == currentYear)
                    {
                        var month = p.CreatedDate.Month - 1;
                        monthlyPatients[month]++;
                        if (p.IsOnline)
                        {
                            monthlyOnlinePatients[month]++;
                        }
                    }
                }

                foreach(var a in appointments)
                {
                    if (a.ScheduledAt.Year == currentYear)
                    {
                        var month = a.ScheduledAt.Month - 1;
                        monthlyAppointments[month]++;
                        monthlyRevenue[month] += (a.Fee ?? 0);
                    }
                }

                ViewBag.ChartDataPatients = string.Join(",", monthlyPatients);
                ViewBag.ChartDataOnlinePatients = string.Join(",", monthlyOnlinePatients);
                ViewBag.ChartDataRevenue = string.Join(",", monthlyRevenue);
                ViewBag.ChartDataAppointments = string.Join(",", monthlyAppointments);
                ViewBag.ChartLabels = "'Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'";
                ViewBag.PeriodLabel = "This Year";
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Export()
        {
            var currentYear = DateTime.UtcNow.Year;

            var patients = await _context.Patients.Find(_ => true).ToListAsync();
            var appointments = await _context.Appointments.Find(_ => true).ToListAsync();

            var monthlyPatients = new int[12];
            var monthlyOnlinePatients = new int[12];
            var monthlyRevenue = new decimal[12];
            var monthlyAppointments = new int[12];

            foreach(var p in patients)
            {
                if (p.CreatedDate.Year == currentYear)
                {
                    var month = p.CreatedDate.Month - 1;
                    monthlyPatients[month]++;
                    if (p.IsOnline)
                    {
                        monthlyOnlinePatients[month]++;
                    }
                }
            }

            foreach(var a in appointments)
            {
                if (a.ScheduledAt.Year == currentYear)
                {
                    var month = a.ScheduledAt.Month - 1;
                    monthlyAppointments[month]++;
                    monthlyRevenue[month] += (a.Fee ?? 0);
                }
            }

            var builder = new StringBuilder();
            builder.AppendLine("Month,Total Patients,Online Patients,Appointments,Revenue (PKR)");
            
            var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            for(int i = 0; i < 12; i++)
            {
                builder.AppendLine($"{months[i]},{monthlyPatients[i]},{monthlyOnlinePatients[i]},{monthlyAppointments[i]},{monthlyRevenue[i]}");
            }
            
            builder.AppendLine();
            builder.AppendLine($"Total,{patients.Count},{patients.Count(p => p.IsOnline)},{appointments.Count},{appointments.Sum(a => a.Fee ?? 0)}");

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"HealthcareCRM_Report_{currentYear}.csv");
        }
    }
}
