using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WoodHome.Data;
using WoodHome.Domain.Entities;
using WoodHome.Models;
using WoodHome.Infrastructure;

namespace WoodHome.Controllers
{
    [Authorize] 
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager; 
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager; 
            _context = context;
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var model = new ProfileViewModel
            {
                Email = user.Email ?? "Не указан",
                FirstName = user.FirstName, 
                LastName = user.LastName,
                Requests = await _context.Requests
                    .Where(r => r.UserId == user.Id)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync()
            };

            return View(model);
        }

    
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}