using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using WoodHome.Domain.Entities;

namespace WoodHome.Models.ViewModels;

public class ProjectFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название проекта")]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(300)]
    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    public string? Area { get; set; }

    public string? ConstructionTime { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsActive { get; set; } = true;

    public IFormFile? MainPhotoFile { get; set; }

    public List<IFormFile> GalleryFiles { get; set; } = new();

    public List<int> ImagesToDelete { get; set; } = new();

    public string? ExistingMainPhoto { get; set; }

    public List<ProjectImage> ExistingImages { get; set; } = new();
}