using System.ComponentModel.DataAnnotations;

namespace SominnercoreDash.Models;

public class ResetPasswordModel
{
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class PasswordStrength
{
    public int Score { get; set; } // 0-4
    public string Label { get; set; } = "Weak";
    public string ColorClass { get; set; } = "bg-red-500";
    public bool HasMinLength { get; set; }
    public bool HasNumber { get; set; }
    public bool HasSpecialChar { get; set; }
    public bool HasUpperCase { get; set; }
    public bool HasLowerCase { get; set; }
}