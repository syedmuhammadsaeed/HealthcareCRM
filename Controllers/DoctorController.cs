using Microsoft.AspNetCore.Mvc;

namespace HealthcareCRM.Controllers
{
    public class DoctorController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
