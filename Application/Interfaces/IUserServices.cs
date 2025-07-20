using DTO.DTO.User;

namespace Application.Interfaces
{
    public interface IUserServices
    {
        Task<RegisterResponseDto> RegisterUserAsync(RegisterRequestDto request);
        Task<string> LoginAsync(LoginRequestDto request);
        public Task LogoutAsync(string token);
    }
}
