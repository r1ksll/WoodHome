using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 
using Microsoft.AspNetCore.Http;

namespace WoodHome.Domain.Entities
{
    public class Service
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название")]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? ShortDescription { get; set; }

        public string? Description { get; set; }

        public string? Photo { get; set; }

        public int ServiceCategoryId { get; set; }
        public ServiceCategory? ServiceCategory { get; set; }

        public bool IsActive { get; set; } = true;

        [NotMapped] 
        public IFormFile? UploadedImage { get; set; } 
    }
}