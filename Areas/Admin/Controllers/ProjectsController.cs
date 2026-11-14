
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WoodHome.Domain.Entities;
using WoodHome.Infrastructure;
using WoodHome.Models.ViewModels;

namespace WoodHome.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Администратор")]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        private readonly string[] _allowedExtensions =
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private const long MaxFileSize = 10 * 1024 * 1024;

        public ProjectsController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.Id)
                .ToListAsync();

            return View(projects);
        }

        public IActionResult Create()
        {
            return View(new ProjectFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectFormViewModel model)
        {
            if (model.MainPhotoFile == null ||
                model.MainPhotoFile.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(model.MainPhotoFile),
                    "Загрузите фотографию проекта.");
            }
            else
            {
                ValidateImage(
                    model.MainPhotoFile,
                    nameof(model.MainPhotoFile));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var project = new Project
            {
                Title = model.Title,
                ShortDescription = model.ShortDescription,
                Description = model.Description,

                Area = AddUnit(model.Area, "м²"),
                ConstructionTime = AddUnit(
                    model.ConstructionTime,
                    "месяцев"),

                IsFeatured = model.IsFeatured,
                IsActive = model.IsActive
            };

            project.MainPhoto = await SaveImageAsync(
                model.MainPhotoFile!);

            _context.Projects.Add(project);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Проект успешно добавлен.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            var model = new ProjectFormViewModel
            {
                Id = project.Id,
                Title = project.Title,
                ShortDescription = project.ShortDescription,
                Description = project.Description,

                Area = RemoveUnit(project.Area, "м²"),
                ConstructionTime = RemoveUnit(
                    project.ConstructionTime,
                    "месяцев"),

                IsFeatured = project.IsFeatured,
                IsActive = project.IsActive,
                ExistingMainPhoto = project.MainPhoto
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ProjectFormViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            if (model.MainPhotoFile != null &&
                model.MainPhotoFile.Length > 0)
            {
                ValidateImage(
                    model.MainPhotoFile,
                    nameof(model.MainPhotoFile));
            }

            if (!ModelState.IsValid)
            {
                model.ExistingMainPhoto = project.MainPhoto;

                return View(model);
            }

            project.Title = model.Title;
            project.ShortDescription = model.ShortDescription;
            project.Description = model.Description;

            project.Area = AddUnit(model.Area, "м²");
            project.ConstructionTime = AddUnit(
                model.ConstructionTime,
                "месяцев");

            project.IsFeatured = model.IsFeatured;
            project.IsActive = model.IsActive;

            if (model.MainPhotoFile != null &&
                model.MainPhotoFile.Length > 0)
            {
                var oldMainPhoto = project.MainPhoto;

                project.MainPhoto = await SaveImageAsync(
                    model.MainPhotoFile);

                DeleteImage(oldMainPhoto);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Изменения успешно сохранены.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            DeleteImage(project.MainPhoto);

            foreach (var image in project.Images)
            {
                DeleteImage(image.ImagePath);
            }

            _context.Projects.Remove(project);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Проект удалён.";

            return RedirectToAction(nameof(Index));
        }

        private void ValidateImage(
            IFormFile file,
            string fieldName)
        {
            var extension = Path
                .GetExtension(file.FileName)
                .ToLowerInvariant();

            if (!_allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    fieldName,
                    "Допустимые форматы: JPG, JPEG, PNG, WEBP.");
            }

            if (file.Length > MaxFileSize)
            {
                ModelState.AddModelError(
                    fieldName,
                    "Размер фотографии не должен превышать 10 МБ.");
            }
        }

        private string? AddUnit(string? value, string unit)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            value = value.Trim();

            if (value.EndsWith(
                unit,
                StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }

            return $"{value} {unit}";
        }

        private string? RemoveUnit(string? value, string unit)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            value = value.Trim();

            if (value.EndsWith(
                unit,
                StringComparison.OrdinalIgnoreCase))
            {
                value = value[..^unit.Length].Trim();
            }

            return value;
        }


        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var extension = Path
                .GetExtension(file.FileName)
                .ToLowerInvariant();

            var fileName = Guid.NewGuid().ToString("N") + extension;

            var folderPath = Path.Combine(
                _environment.WebRootPath,
                "img",
                "portfolio");

            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/img/portfolio/" + fileName;
        }

 
        private void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }


            const string prefix = "/img/portfolio/";

            if (!imagePath.StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var fileName = Path.GetFileName(imagePath);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            var folderPath = Path.GetFullPath(
                Path.Combine(
                    _environment.WebRootPath,
                    "img",
                    "portfolio"));

            var filePath = Path.GetFullPath(
                Path.Combine(folderPath, fileName));

            if (!filePath.StartsWith(
                folderPath + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
