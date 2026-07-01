using Microsoft.AspNetCore.Mvc;

namespace HealthcareCRM.Controllers
{
    /// <summary>
    /// MVC controller that serves Razor views for authentication (Login, Register).
    /// Data operations are performed client-side via the API controllers using JWT.
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Displays the login page.
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Displays the user registration page.
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
    }
}
