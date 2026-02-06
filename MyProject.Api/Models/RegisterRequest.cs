using System.ComponentModel.DataAnnotations;

namespace MyProject.Api.Models;

public sealed class RegisterRequest
{
    [Required]
    public string UserName { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

