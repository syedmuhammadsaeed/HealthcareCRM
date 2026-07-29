using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace HealthcareCRM.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class OnlinePatientController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
