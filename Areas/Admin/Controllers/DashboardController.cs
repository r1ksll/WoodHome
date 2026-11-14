using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WoodHome.Domain.Entities;
using WoodHome.Infrastructure;

namespace WoodHome.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Администратор,Менеджер")] 
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; 

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager; 
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalServices = await _context.Services.CountAsync();
            ViewBag.TotalProjects = await _context.Projects.CountAsync();
            ViewBag.TotalReviews = await _context.Reviews.CountAsync();
            ViewBag.PendingReviews = await _context.Reviews.CountAsync(r => !r.IsApproved);
            ViewBag.TotalRequests = await _context.Requests.CountAsync();

            ViewBag.TotalUsers = await _userManager.Users.CountAsync(); 

            return View();
        }
    }
}