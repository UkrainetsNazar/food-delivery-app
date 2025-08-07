using System.ComponentModel.DataAnnotations;

namespace Auth.API.Domain.DTO;

public class RegisterDto
{
    [Required]
    [EmailAddress(ErrorMessage = "Email must looks like: example@gmail.com")]
    public string? Email { get; set; }
    [Required]
    [StringLength(30, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 20 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number")]
    public string? Password { get; set; }
    [Required]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 30 characters")]
    public string? FirstName { get; set; }
    [Required]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 30 characters")]
    public string? LastName { get; set; }
}