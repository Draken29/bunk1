using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using MyProject.Api.Models;
using MyProject.Api.Services;

namespace MyProject.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UserController : ControllerBase
{
    private readonly IUserStore _userStore;

    public UserController(IUserStore userStore)
    {
        _userStore = userStore;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        var normalizedUserName = NormalizeUserName(request.UserName);
        if (string.IsNullOrWhiteSpace(normalizedUserName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new AuthResponse { Message = "UserName and Password are required." });
        }

        if (!IsValidUserName(request.UserName))
        {
            return BadRequest(new AuthResponse
            {
                Message = "UserName must contain at least one uppercase and one lowercase letter."
            });
        }

        if (!IsValidPassword(request.Password))
        {
            return BadRequest(new AuthResponse
            {
                Message = "Password must contain letters, numbers, and special characters."
            });
        }

        var passwordHash = HashPassword(request.Password);
        if (!_userStore.TryAddPending(normalizedUserName, passwordHash))
        {
            return Conflict(new AuthResponse { Message = "UserName already exists." });
        }

        return Ok(new AuthResponse { Message = "Registration pending admin approval." });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var normalizedUserName = NormalizeUserName(request.UserName);
        if (string.IsNullOrWhiteSpace(normalizedUserName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new AuthResponse { Message = "UserName and Password are required." });
        }

        var passwordHash = HashPassword(request.Password);
        if (_userStore.TryValidatePending(normalizedUserName, passwordHash))
        {
            return Unauthorized(new AuthResponse { Message = "Account pending admin approval." });
        }

        if (!_userStore.TryValidate(normalizedUserName, passwordHash))
        {
            return Unauthorized(new AuthResponse { Message = "Invalid credentials." });
        }

        return Ok(new AuthResponse { Message = "Logged in successfully." });
    }

    [HttpGet("pending")]
    public IActionResult Pending()
    {
        return Ok(_userStore.GetPendingUsers());
    }

    [HttpPost("approve")]
    public IActionResult Approve([FromBody] AdminApprovalRequest request)
    {
        if (!request.Admin)
        {
            return BadRequest(new AuthResponse { Message = "Admin approval is required." });
        }

        var normalizedUserName = NormalizeUserName(request.UserName);
        if (string.IsNullOrWhiteSpace(normalizedUserName))
        {
            return BadRequest(new AuthResponse { Message = "UserName is required." });
        }

        if (!_userStore.TryApprove(normalizedUserName))
        {
            return NotFound(new AuthResponse { Message = "Pending user not found." });
        }

        return Ok(new AuthResponse { Message = "User approved successfully." });
    }

    private static string NormalizeUserName(string userName)
    {
        return userName.Trim().ToLowerInvariant();
    }

    private static bool IsValidUserName(string userName)
    {
        var hasUpper = false;
        var hasLower = false;

        foreach (var ch in userName)
        {
            if (char.IsUpper(ch))
            {
                hasUpper = true;
            }
            else if (char.IsLower(ch))
            {
                hasLower = true;
            }

            if (hasUpper && hasLower)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsValidPassword(string password)
    {
        var hasLetter = false;
        var hasDigit = false;
        var hasSpecial = false;

        foreach (var ch in password)
        {
            if (char.IsLetter(ch))
            {
                hasLetter = true;
            }
            else if (char.IsDigit(ch))
            {
                hasDigit = true;
            }
            else
            {
                hasSpecial = true;
            }

            if (hasLetter && hasDigit && hasSpecial)
            {
                return true;
            }
        }

        return false;
    }

    private static string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}

