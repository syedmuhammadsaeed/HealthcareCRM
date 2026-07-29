using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthcareCRM.Controllers
{
    public class BillingController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "Admin" && role != "SuperAdmin")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
    }
}
