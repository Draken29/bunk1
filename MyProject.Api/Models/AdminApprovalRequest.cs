using System.ComponentModel.DataAnnotations;

namespace MyProject.Api.Models;

public sealed class AdminApprovalRequest
{
    [Required]
    public string UserName { get; init; } = string.Empty;

    public bool Admin { get; init; }
}

