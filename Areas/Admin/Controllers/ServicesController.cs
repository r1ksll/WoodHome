using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WoodHome.Domain.Entities;
using WoodHome.Infrastructure;

namespace WoodHome.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Администратор")] 
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var services = await _context.Services.Include(s => s.ServiceCategory).ToListAsync();
            return View(services);
        }

public IActionResult Create()
{
    var cats = _context.ServiceCategories.ToList();
    
    
    Console.WriteLine($"--- КАТЕГОРИЙ В БАЗЕ: {cats.Count} ---"); 
    
    ViewBag.Categories = cats;
    return View(new Service());
}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (ModelState.IsValid)
            {
                if (service.UploadedImage != null && service.UploadedImage.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(service.UploadedImage.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await service.UploadedImage.CopyToAsync(stream);
                    }
                    service.Photo = "/img/" + fileName;
                }

                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = _context.ServiceCategories.ToList();
            return View(service);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            ViewBag.Categories = _context.ServiceCategories.ToList();
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service service)
        {
            if (id != service.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (service.UploadedImage != null && service.UploadedImage.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(service.UploadedImage.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
                        
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await service.UploadedImage.CopyToAsync(stream);
                        }
                        service.Photo = "/img/" + fileName;
                    }
                    else
                    {
                        var existingService = await _context.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
                        service.Photo = existingService?.Photo;
                    }

                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = _context.ServiceCategories.ToList();
            return View(service);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}