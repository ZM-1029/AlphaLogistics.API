using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Model;

namespace AlphaLogistics.API.Services
{
    public interface IUserService
    {
        // User operations

        public Task<object> ActiveRoles();
        public  Task<int> UserCount(int? roleId);
        public  Task<dynamic> GetCustomerByIdAsync(int customerId);
        public  Task<List<dynamic>> GetAllCustomerAsync();
        public  Task<bool> UpdateCustomerAsync(CustomerCreateDTO registerDto);
        public  Task<bool> RegisterCustomerAsync(CustomerCreateDTO registerDto);
        public List<PradeshMaster> GetActivePradeshList();
        Task<UserResponseDto> RegisterUserAsync(RegisterUserDto registerDto);
        Task<UserResponseDto> GetUserByIdAsync(int id);
        Task<List<UserResponseDto>> GetAllUsersAsync(int? roleId, int page, int pageSize);
        Task<VendorMaster?> GetVendorByUserId(int userId);
        Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserDto updateDto);
        Task<UserResponseDto> GetCurrentUserAsync(HttpContext httpContext);

        Task<string> LoginAsync(LoginDto loginDto, HttpContext httpContext);
        Task<bool> LogoutAsync(HttpContext httpContext);

        
        Task<VendorResponseDto> RegisterVendorAsync(RegisterVendorDto registerDto,HttpContext httpContext);

        Task<VendorListResponseDto> GetAllVendorsAsync(VendorQueryDto dto);
        Task<VendorResponseDto> GetVendorByIdAsync(int vendorId);
        Task<VendorResponseDto> UpdateVendorAsync(int vendorId, UpdateVendorDto updateDto, HttpContext httpContext);
        Task<VendorResponseDto> ApproveOrRejectVendorAsync(int vendorId, bool isApproved, HttpContext httpContext, string? reason);
        Task<bool> DeleteVendorAsync(int vendorId);
        Task<bool> RestoreVendorAsync(int vendorId);
        // Vendor documents
        Task<List<VendorDocumentDto>> GetVendorDocumentsAsync(int vendorId);
        Task<VendorDocumentDto> AddVendorDocumentAsync(int vendorId, AddVendorDocumentDto addDocumentDto);
        Task<bool> DeleteVendorDocumentAsync(int documentId);
    }
}
