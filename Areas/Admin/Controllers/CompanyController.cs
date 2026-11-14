
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WoodHome.Domain.Entities;
using WoodHome.Infrastructure;
using System.IO;

namespace WoodHome.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Администратор")]
    public class CompanyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompanyController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Edit()
        {
            var company = await _context.CompanyInfos.FirstOrDefaultAsync();

            if (company == null)
            {
                company = new CompanyInfo { CompanyName = "WoodHome" };
            }

            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CompanyInfo model, IFormFile? uploadedImage)
        {
            if (ModelState.IsValid)
            {
                if (uploadedImage != null && uploadedImage.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedImage.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadedImage.CopyToAsync(stream);
                    }

                    model.ImagePath = "/img/" + fileName;
                }

                var existing = await _context.CompanyInfos
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    model.Id = existing.Id;
                    _context.Update(model);
                }
                else
                {
                    _context.Add(model);
                }

                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Dashboard");
            }

            return View(model);
        }
    }
}
