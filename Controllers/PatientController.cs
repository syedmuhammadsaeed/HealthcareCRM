using Microsoft.AspNetCore.Mvc;

namespace HealthcareCRM.Controllers
{
    /// <summary>
    /// MVC controller that serves Razor views for patient management.
    /// Data is loaded client-side via the /api/patients API using JWT.
    /// </summary>
    public class PatientController : Controller
    {
        /// <summary>
        /// Displays the patient list page.
        /// JavaScript will redirect to /Account/Login if no JWT is present.
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Displays the patient creation form.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View("Form");
        }

        /// <summary>
        /// Displays the patient edit form pre-loaded with the given patient's data.
        /// </summary>
        /// <param name="id">MongoDB ObjectId string of the patient to edit.</param>
        [HttpGet]
        public IActionResult Edit(string id)
        {
            ViewBag.PatientId = id;
            return View("Form");
        }
    }
}
