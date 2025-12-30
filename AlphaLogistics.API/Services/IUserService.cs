using AlphaLogistics.API.DTO;

namespace AlphaLogistics.API.Services
{
    public interface IUserService
    {
        // User operations
        Task<UserResponseDto> RegisterUserAsync(RegisterUserDto registerDto);
        Task<UserResponseDto> GetUserByIdAsync(int id);
        Task<List<UserResponseDto>> GetAllUsersAsync(int? roleId = null);
        Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserDto updateDto);
        Task<UserResponseDto> GetCurrentUserAsync(HttpContext httpContext);

        Task<string> LoginAsync(LoginDto loginDto, HttpContext httpContext);
        Task<bool> LogoutAsync(HttpContext httpContext);

        
        Task<VendorResponseDto> RegisterVendorAsync(RegisterVendorDto registerDto,HttpContext httpContext);

        Task<List<VendorResponseDto>> GetAllVendorsAsync(bool? isActive = null, bool? isApproved = null);
        Task<VendorResponseDto> GetVendorByIdAsync(int vendorId);
        Task<VendorResponseDto> UpdateVendorAsync(int vendorId, UpdateVendorDto updateDto, HttpContext httpContext);
        Task<bool> DeleteVendorAsync(int vendorId);
        Task<bool> RestoreVendorAsync(int vendorId);
        Task<bool> ApproveVendorAsync(int vendorId, int approvedByUserId);
        Task<bool> RejectVendorAsync(int vendorId, int rejectedByUserId);

        // Vendor documents
        Task<List<VendorDocumentDto>> GetVendorDocumentsAsync(int vendorId);
        Task<VendorDocumentDto> AddVendorDocumentAsync(int vendorId, AddVendorDocumentDto addDocumentDto);
        Task<bool> DeleteVendorDocumentAsync(int documentId);
    }
}
