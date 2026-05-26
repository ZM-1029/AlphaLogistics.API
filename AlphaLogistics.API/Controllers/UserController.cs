using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Model;
using AlphaLogistics.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Threading.Tasks;
using WALMS.API.Common;
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

        #region Customer APIs
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterCustomer(CustomerCreateDTO registerDto) // For customer registration
        {
            try
            {
                var userId = await _userService.RegisterCustomerAsync(registerDto);
                return SuccessResponse(new { UserId = userId }, "customer registered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                return ErrorResponse<string>(ex.Message);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateCustomer(CustomerCreateDTO registerDto)
        {
            try
            {
                var result = await _userService.UpdateCustomerAsync(registerDto);
                return SuccessResponse(result, "customer updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                return ErrorResponse<string>(ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCustomerList()
        {
            var customerList = await _userService.GetAllCustomerAsync();
            if (!customerList.Any()) return NoContentResponse<string>("No customer found!");
            return SuccessResponse(customerList, "Customer retrieved successfully");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCustomerById(int customerId)
        {
            if (customerId <= 0) return ErrorResponse<string>("Invalid inputs");
            var customer = await _userService.GetCustomerByIdAsync(customerId);
            if (customer == null) return NoContentResponse<string>("No customer found!");
            return SuccessResponse(customer, "Customer retrieved successfully");
        }

        #endregion

        #region Pradesh APIs
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ActivePradeshList()
        {
            var pradeshList = _userService.GetActivePradeshList();
            if (!pradeshList.Any()) return NoContentResponse<string>("No active pradesh found");

            var response = pradeshList
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    p.Charge,
                })
                .OrderBy(p => p.Name)
                .ToList();

            return SuccessResponse(response, "Active pradesh list retrieved successfully");
        }
        #endregion

        # region User APIs
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromForm] RegisterUserDto registerDto) // For Internal users-> Admin, Finance etc
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
       /* public async Task<IActionResult> GetAllUsers([FromQuery] int? roleId = null)
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
        }*/
        public async Task<IActionResult> GetAllUsers(int? roleId, int page, int pageSize)
        {
            try
            {
                var users = await _userService.GetAllUsersAsync(roleId,page,pageSize);
                var totalCount = await _userService.UserCount(roleId);
                var response = new { TotalCount= totalCount , users= users };
                return SuccessResponse(response);
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
                var vendor= await _userService.GetVendorByUserId(user.Id);
                var response = new {
                    Id=user.Id,
                    Role= user.Role,
                    UserName = user.UserName,
                    Email= user.Email,
                    Phone=user.Phone,
                    ProfileImage=user.ProfileImage,
                    VendorId = vendor?.Id??0
                };
                return SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return UnauthorizedResponse<string>(ex.Message);
            }
        }

        #endregion

        #region Vendor APIs
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

        [HttpPatch("{vendorId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]   
        public async Task<IActionResult> VendorApprovalUpdate(int vendorId, [FromBody] VendorApprovalRequestDto request)
        {
            try
            {
                var result = await _userService.ApproveOrRejectVendorAsync(
                    vendorId,
                    request.IsApproved,
                    HttpContext,
                    request.Reason
                );

               

                var response = new VendorApprovalResponseDto
                {
                    VendorId = result.VendorId,
                    VendorName = result.VendorName,
                    IsApproved = result.IsApproved,
                    IsActive = result.IsActive,
                    StatusMessage = request.IsApproved ? "Vendor approved successfully" : "Vendor rejected",
                    ActionDate = DateTime.UtcNow,
                    ActionByUserId = result.UpdatedBy
                };

                return Ok(new ApiResponse<VendorApprovalResponseDto>
                {
                    Success = true,
                    Data = response,
                    Message = response.StatusMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vendor approval status");
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Error updating vendor approval status" });
            }
        }

        [HttpPost]
        //[Authorize(Policy = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAllVendors([FromBody] VendorQueryDto query)
        {
            try
            {
                var result = await _userService.GetAllVendorsAsync(query);

                return SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vendors");
                return ConflictResponse<string>(ex.Message); 
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveVendor()
        {
            try
            {
                var result = await _userService.GetActiveVendor();
                if (result == null) return NoContentResponse<string>("No active or approved vendor found");
                return SuccessResponse(result, "vendor retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return ErrorResponse<string>(ex.Message);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveUser()
        {
            try
            {
                var result = await _userService.GetActiveUsers();
                if (result == null) return NoContentResponse<string>("No active user found");
                return SuccessResponse(result, "User retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return ErrorResponse<string>(ex.Message);
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

        #endregion

        [HttpGet]
        public async Task<IActionResult> GetActiveRoles()
        {
            var roles = await _userService.ActiveRoles();

            if (roles == null) return NoContentResponse<string>("No active role found!");

            return SuccessResponse(roles,"Data retrieved successfully!");
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