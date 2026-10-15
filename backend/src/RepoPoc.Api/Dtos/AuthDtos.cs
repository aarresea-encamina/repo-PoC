using System.ComponentModel.DataAnnotations;

namespace RepoPoc.Api.Dtos;

public record LoginRequest(
    [Required, MaxLength(50)] string Username,
    [Required, MaxLength(100)] string Password);

public record LoginResponse(string Token, DateTime ExpiresAt, UserDto User);
