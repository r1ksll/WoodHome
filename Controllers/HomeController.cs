using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WoodHome.Models;
using WoodHome.Infrastructure;
using Microsoft.EntityFrameworkCore;
using WoodHome.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace WoodHome.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeViewModel
        {
            Company = await _context.CompanyInfos.FirstOrDefaultAsync(),

            Slides = await _context.HomeSlides
                .Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder)
                .ToListAsync(),

            Services = await _context.Services
                .Where(s => s.IsActive)
                .Include(s => s.ServiceCategory)
                .ToListAsync(),

            Projects = await _context.Projects
                .Where(p => p.IsActive && p.IsFeatured)
                .Include(p => p.Images)
                .ToListAsync(),

            Reviews = await _context.Reviews
                .Where(r => r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .Take(3)
                .ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> CompanyInfo()
{
    var companyInfo = await _context.CompanyInfos
        .AsNoTracking()
        .FirstOrDefaultAsync();
    return View(companyInfo); 
}

    public async Task<IActionResult> Services()
    {
        var categories = await _context.ServiceCategories
            .Include(c => c.Services.Where(s => s.IsActive))
            .ToListAsync();

        ViewBag.Categories = categories;

        return View(categories);
    }

    public async Task<IActionResult> Portfolio()
    {
        var projects = await _context.Projects
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Include(p => p.Images)
            .OrderByDescending(p => p.IsFeatured)
            .ThenBy(p => p.Id)
            .ToListAsync();

        return View(projects);
    }

    public async Task<IActionResult> Reviews()
    {
        var reviews = await _context.Reviews
            .Where(r => r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(reviews);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(int rating, string text)
    {
        if (rating < 1 || rating > 5 ||
            string.IsNullOrWhiteSpace(text) ||
            text.Length > 2000)
        {
            TempData["ReviewError"] = "Проверьте оценку и текст отзыва.";
            return RedirectToAction(nameof(Reviews));
        }

        var review = new Review
        {
            AuthorName = User.Identity?.Name ?? "Пользователь",
            Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
            Text = text.Trim(),
            Rating = rating,
            CreatedAt = DateTime.UtcNow,
            IsApproved = false,
            UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        TempData["ReviewSuccess"] =
            "Спасибо за отзыв! Он появится на сайте после проверки модератором.";

        return RedirectToAction(nameof(Reviews));
    }

    public async Task<IActionResult> Contacts()
    {
        var company = await _context.CompanyInfos
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return View(company);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendRequest(
        [Bind("ClientName,Phone,Email,Message")] Request request)
    {
        if (!ModelState.IsValid)
        {
            var company = await _context.CompanyInfos
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return View("Contacts", company);
        }

        request.CreatedAt = DateTime.UtcNow;
        request.IsProcessed = false;

        if (User.Identity?.IsAuthenticated == true)
        {
            request.UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        _context.Requests.Add(request);
        await _context.SaveChangesAsync();

        TempData["RequestSuccess"] =
            "Ваша заявка успешно отправлена! Мы скоро свяжемся с вами.";

        return RedirectToAction(nameof(Contacts));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}