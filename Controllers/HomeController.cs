using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HealthcareCRM.Models;
using HealthcareCRM.Data;
using MongoDB.Driver;

namespace HealthcareCRM.Controllers;

public class HomeController : Controller
{
    private readonly MongoDbContext _context;

    public HomeController(MongoDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        // Gather dashboard statistics
        var total = _context.Patients.CountDocuments(FilterDefinition<Patient>.Empty);
        var active = _context.Patients.CountDocuments(p => p.Status == "active" || p.Status == "Active");
        var followup = _context.Patients.CountDocuments(p => p.Status == "follow-up" || p.Status == "followup" || p.Status == "Follow-up" || p.Status == "Followup");
        var critical = _context.Patients.CountDocuments(p => p.Status == "critical" || p.Status == "Critical");

        // Get 5 most recently created patients for the table
        var recentPatients = _context.Patients
            .Find(FilterDefinition<Patient>.Empty)
            .SortByDescending(p => p.CreatedDate)
            .Limit(5)
            .ToList();

        var model = new OverviewViewModel
        {
            TotalPatients = total,
            ActivePatients = active,
            FollowUpsDue = followup,
            CriticalCases = critical,
            RecentPatients = recentPatients
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
