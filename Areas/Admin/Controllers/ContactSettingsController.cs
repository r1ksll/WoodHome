
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WoodHome.Domain.Entities;
using WoodHome.Infrastructure;
using WoodHome.Models.ViewModels;

namespace WoodHome.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Администратор")]
public class ContactSettingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ContactSettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var company = await _context.CompanyInfos.FirstOrDefaultAsync();

        var model = new ContactSettingsViewModel
        {
            Address = company?.Address,
            Phone = company?.Phone,
            Email = company?.Email,
            MapUrl = company?.MapUrl
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ContactSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var company = await _context.CompanyInfos.FirstOrDefaultAsync();

        if (company == null)
        {
            company = new CompanyInfo
            {
                CompanyName = "WoodHome"
            };

            _context.CompanyInfos.Add(company);
        }

        company.Address = model.Address;
        company.Phone = model.Phone;
        company.Email = model.Email;
        company.MapUrl = model.MapUrl;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Контактные данные успешно сохранены.";

        return RedirectToAction(nameof(Index));
    }
}
