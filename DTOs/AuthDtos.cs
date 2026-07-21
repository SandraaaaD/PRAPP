using System.ComponentModel.DataAnnotations;

namespace BenchApp.DTOs;

// Step 1 of registration: full name, email, password
public class RegisterStep1Dto
{
    [Required(ErrorMessage = "Full name is required.")]
    [MinLength(3, ErrorMessage = "Full name must be at least 3 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email format is invalid.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    public string Password { get; set; } = string.Empty;
}

// Step 2 of registration: specialty / oblast / nasoka
public class RegisterStep2Dto
{
    [Required(ErrorMessage = "User id is required.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Specialty is required.")]
    public int SpecialtyId { get; set; }

    [Required(ErrorMessage = "Direction is required.")]
    public string Direction { get; set; } = string.Empty;

    public string? Bio { get; set; }
}

public class LoginDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email format is invalid.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public bool IsProfileComplete { get; set; }
    public string Role { get; set; } = string.Empty;
}
