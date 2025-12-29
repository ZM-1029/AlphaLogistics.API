using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Model;
using AlphaLogistics.API.Services;
using Microsoft.AspNetCore.Authorization;
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
        [HttpPost("register")]
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

        [HttpPost("register-vendor")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterVendor([FromForm] RegisterVendorDto registerDto)
        {
            try
            {
                var result = await _userService.RegisterVendorAsync(registerDto);
                return CreatedResponse(result, "Vendor registered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering vendor");
                return ConflictResponse<string>(ex.Message);
            }
        }

        // POST: api/User/Login
        [HttpPost("login")]
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

        [HttpGet("all")]
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

        [HttpPost("logout")]
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

        [HttpGet("current-user")]
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

        [HttpGet("vendors")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> GetAllVendors([FromQuery] bool? isActive = null)
        {
            try
            {
                var vendors = await _userService.GetAllVendorsAsync(isActive);
                return SuccessResponse(vendors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vendors");
                return ErrorResponse<string>(ex.Message);
            }
        }

        [HttpDelete("delete-vendor/{userId}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> DeleteVendor(int userId)
        {
            try
            {
                await _userService.DeleteVendorAsync(userId);
                return SuccessResponse<string>("", "Vendor deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting vendor with user id {userId}");
                return ErrorResponse<string>(ex.Message);
            }
        }

        [HttpPost("restore-vendor/{userId}")]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> RestoreVendor(int userId)
        {
            try
            {
                await _userService.RestoreVendorAsync(userId);
                return SuccessResponse<string>("", "Vendor restored successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error restoring vendor with user id {userId}");
                return ErrorResponse<string>(ex.Message);
            }
        }

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
    }
}