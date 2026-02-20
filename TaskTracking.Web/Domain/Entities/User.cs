using System;
using System.ComponentModel.DataAnnotations;

namespace TaskTracking.Web.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AuthKey { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "Имя обязательно для заполнения")]
    [StringLength(100, ErrorMessage = "Имя не должно превышать 100 символов")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обязателен для заполнения")]
    [EmailAddress(ErrorMessage = "Введите корректный email адрес")]
    [StringLength(100, ErrorMessage = "Email не должен превышать 100 символов")]
    public string Email { get; set; } = string.Empty;
}
