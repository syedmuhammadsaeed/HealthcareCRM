using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthcareCRM.Controllers
{
    public class DoctorController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role == "Admin" || role == "SuperAdmin")
            {
                return RedirectToAction("List");
            }
            return View();
        }

        [HttpGet]
        public IActionResult List()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role == "Doctor")
            {
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
