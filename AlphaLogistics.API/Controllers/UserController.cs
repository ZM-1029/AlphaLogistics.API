using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Model;
using AlphaLogistics.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WALMS.API.Controllers;

namespace AlphaLogistics.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : BaseController 
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // POST: api/User/Register
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromForm] RegisterUserDto registerDto)
        {
            try
            {
                var result = await _userService.RegisterUserAsync(registerDto);
                return SuccessResponse(result, "User registered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                return ErrorResponse<string>(ex.Message);
            }
        }


        // POST: api/User/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _userService.LoginAsync(loginDto, HttpContext);
                return SuccessResponse(result, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return UnauthorizedResponse<string>(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return SuccessResponse(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user by id {id}");
                return NoContentResponse<string>(ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllUsers([FromQuery] int? roleId = null)
        {
            try
            {
                var users = await _userService.GetAllUsersAsync(roleId);
                return SuccessResponse(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return ErrorResponse<string>(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, [FromForm] UpdateUserDto updateDto)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(id, updateDto);
                return SuccessResponse(user, "User updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user with id {id}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _userService.LogoutAsync(HttpContext);
                return SuccessResponse<string>("", "Logged out successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return ErrorResponse<string>(ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync(HttpContext);
                return SuccessResponse(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return UnauthorizedResponse<string>(ex.Message);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterVendor([FromForm] RegisterVendorDto registerDto)
        {
            try
            {
                
                var result = await _userService.RegisterVendorAsync(registerDto, HttpContext);

                string message = result.IsApproved
                    ? "Vendor registered and approved successfully."
                    : "Vendor registration submitted. Waiting for admin approval.";

                return CreatedResponse(result, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering vendor");
                return ConflictResponse<string>(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateVendor(int id, [FromForm] UpdateVendorDto updateDto)
        {
            try
            {
                var result = await _userService.UpdateVendorAsync(id, updateDto, HttpContext);
                return SuccessResponse(result, "Vendor updated successfully.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to update vendor");
                return UnauthorizedResponse<string>(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vendor");
                return ConflictResponse<string>(ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetAllVendors(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] bool? isActive = null,
    [FromQuery] bool? isApproved = null,
    [FromQuery] string? customerType = null,
    [FromQuery] string? vendorName = null,
    [FromQuery] string? search = null)
        {
            try
            {
                var result = await _userService.GetAllVendorsAsync(
                    pageNumber: page,
                    pageSize: pageSize,
                    isActive: isActive,
                    isApproved: isApproved,
                    customerType: customerType,
                    vendorName: vendorName,
                    searchQuery: search);

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vendors");
                return ConflictResponse<string>(ex.Message);
            }
        }

        [HttpGet("{vendorId}")]
        [Authorize]
        public async Task<IActionResult> GetVendorById(int vendorId)
        {
            try
            {
                var vendor = await _userService.GetVendorByIdAsync(vendorId);
                return SuccessResponse(vendor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting vendor {vendorId}");
                return NoContentResponse<string>(ex.Message);
            }
        }

        [HttpDelete("{vendorId}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteVendor(int vendorId)
        {
            try
            {
                await _userService.DeleteVendorAsync(vendorId);
                return SuccessResponse<string>("", "Vendor deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting vendor {vendorId}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        [HttpPost("{vendorId}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> RestoreVendor(int vendorId)
        {
            try
            {
                await _userService.RestoreVendorAsync(vendorId);
                return SuccessResponse<string>("", "Vendor restored successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error restoring vendor {vendorId}");
                return ErrorResponse<string>(ex.Message);
            }
        }


        [HttpGet("{vendorId}")]
        [Authorize]
        public async Task<IActionResult> GetVendorDocuments(int vendorId)
        {
            try
            {
                var documents = await _userService.GetVendorDocumentsAsync(vendorId);
                return SuccessResponse(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting documents for vendor {vendorId}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        /*
        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserDto updateDto)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(HttpContext);
                var user = await _userService.UpdateUserAsync(currentUser.Id, updateDto);
                return SuccessResponse(user, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return ErrorResponse<string>(ex.Message);
            }
        }
        */
    }
}