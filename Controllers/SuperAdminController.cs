using HealthcareCRM.Interfaces;
using HealthcareCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthcareCRM.Controllers
{
    [Authorize]
    public class SuperAdminController : Controller
    {
        private readonly IUserRepository _userRepository;

        public SuperAdminController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            var pendingUsers = await _userRepository.GetAllPendingAsync();
            return View(pendingUsers);
        }

        public IActionResult Profile()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ApproveUser(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user != null && user.Status == "Pending")
            {
                user.Status = "Approved";
                await _userRepository.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RejectUser(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user != null && user.Status == "Pending")
            {
                user.Status = "Rejected";
                await _userRepository.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }
    }
}
