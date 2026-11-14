
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WoodHome.Infrastructure;

namespace WoodHome.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Администратор,Менеджер")]
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _context.Requests
                .OrderBy(r => r.IsProcessed)
                .ThenByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleProcessed(int id)
        {
            var request = await _context.Requests
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            request.IsProcessed = !request.IsProcessed;

            await _context.SaveChangesAsync();

            TempData["Success"] = request.IsProcessed
                ? "Обращение отмечено как обработанное."
                : "Обращение возвращено в новые.";

            return RedirectToAction(nameof(Index));
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.Requests
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            _context.Requests.Remove(request);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Обращение удалено.";

            return RedirectToAction(nameof(Index));
        }
    }
}
