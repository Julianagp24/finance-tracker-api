namespace FinanceTrackerApi.Dtos;

public record AuthResponse(
    int UserId,
    string Email,
    string Token
);