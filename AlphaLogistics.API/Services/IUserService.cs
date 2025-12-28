using AlphaLogistics.API.DTO;

namespace AlphaLogistics.API.Services
{
    public interface IUserService
    {
        Task<UserResponseDto> RegisterUserAsync(RegisterUserDto registerDto);
        Task<UserResponseDto> RegisterVendorAsync(RegisterVendorDto registerDto); 
        Task<string> LoginAsync(LoginDto loginDto, HttpContext httpContext);
        Task<UserResponseDto> GetUserByIdAsync(int id); 
        Task<List<UserResponseDto>> GetAllUsersAsync(int? roleId = null);
        Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserDto updateDto); 
        Task<bool> LogoutAsync(HttpContext httpContext);
        Task<UserResponseDto> GetCurrentUserAsync(HttpContext httpContext);

       
        Task<List<UserResponseDto>> GetAllVendorsAsync(bool? isActive = null);
        Task<bool> DeleteVendorAsync(int userId); 
        Task<bool> RestoreVendorAsync(int userId);
    }
}
