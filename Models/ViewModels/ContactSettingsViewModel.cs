using System.ComponentModel.DataAnnotations;

namespace WoodHome.Models.ViewModels;

public class ContactSettingsViewModel
{
    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    [EmailAddress(ErrorMessage = "Введите корректный адрес электронной почты")]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? MapUrl { get; set; }
}