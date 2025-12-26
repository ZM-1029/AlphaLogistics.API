using AlphaLogistics.API.Model;
using AlphaLogistics.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WALMS.API.Controllers;

namespace AlphaLogistics.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromForm] RegisterUserDto registerDto)
    {
        try
        {
            var result = await _userService.RegisterUserAsync(registerDto);
            return Ok(new { success = true, data = result, message = "User registered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> RegisterVendor([FromForm] RegisterVendorDto registerDto)
    {
        try
        {
            var result = await _userService.RegisterVendorAsync(registerDto);
            return Ok(new { success = true, data = result, message = "Vendor registered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering vendor");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var result = await _userService.LoginAsync(loginDto, HttpContext);
            return Ok(new { success = true, message = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return Unauthorized(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var user = await _userService.GetCurrentUserAsync(HttpContext);
            return Ok(new { success = true, data = user });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return Unauthorized(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllUsers([FromQuery] int? roleId = null)
    {
        try
        {
            var users = await _userService.GetAllUsersAsync(roleId);
            return Ok(new { success = true, data = users });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetUserById(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(new { success = true, data = user });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by id");
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(int id, [FromForm] UpdateUserDto updateDto)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, updateDto);
            return Ok(new { success = true, data = user, message = "User updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _userService.LogoutAsync(HttpContext);
            return Ok(new { success = true, message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

