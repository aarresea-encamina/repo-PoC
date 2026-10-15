using System.ComponentModel.DataAnnotations;
using RepoPoc.Api.Models;

namespace RepoPoc.Api.Dtos;

public record UserDto(
    int Id,
    string Username,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static UserDto FromEntity(User user) => new(
        user.Id, user.Username, user.FirstName, user.LastName, user.Email, user.Phone, user.CreatedAt, user.UpdatedAt);
}

public class CreateUserRequest
{
    [Required, StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone, MaxLength(20)]
    public string? Phone { get; set; }
}

public class UpdateUserRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone, MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>Optional. When provided, the user's password is replaced.</summary>
    [StringLength(100, MinimumLength = 6)]
    public string? Password { get; set; }
}
