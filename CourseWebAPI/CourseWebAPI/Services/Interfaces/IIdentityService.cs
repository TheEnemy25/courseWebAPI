using CourseWebAPI.Dto.Auth;

namespace CourseWebAPI.Services.Interfaces
{
    public interface IIdentityService
    {
        Task<(bool isSuccessful, AuthenticationResultDto authResult)> RegisterAsync(RegistrationDto registrationDto);
        Task<(bool isSuccessful, AuthenticationResultDto authResult)> LoginAsync(LoginDto loginDto);
        Task<(bool IsSuccessful, AuthenticationResultDto authResult)> RefreshAsync(RefreshTokenDto refreshTokenDto);
    }
}
