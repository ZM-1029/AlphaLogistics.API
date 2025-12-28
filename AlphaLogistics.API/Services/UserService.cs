using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AlphaLogistics.API.Services
{
    public class UserService : IUserService
    {
        private readonly AlphaLogisticsContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(AlphaLogisticsContext context, IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        // Helper method to hash password
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        
        private async Task<string?> UploadProfileImage(IFormFile? profileImage)
        {
            if (profileImage == null || profileImage.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(profileImage.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(fileStream);
            }

            return $"/uploads/profiles/{uniqueFileName}";
        }

        
        private UserResponseDto ConvertToUserResponseDto(UserMaster user)
        {
            var response = new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.RoleMaster?.Name ?? "User",
                ProfileImage = user.ProfileImage,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            if (user.RoleMaster?.Name == "Vendor" && user.VendorMaster != null)
            {
                response.VendorName = user.VendorMaster.Name;
                response.ContactPerson = user.VendorMaster.ContactPerson;
                response.VendorPhone = user.VendorMaster.Phone;
                response.VendorEmail = user.VendorMaster.Email;
                response.VendorAddress = user.VendorMaster.Address;
            }

            return response;
        }

        public async Task<UserResponseDto> RegisterUserAsync(RegisterUserDto registerDto)
        {
           
            if (await _context.UserMasters.AnyAsync(u => u.Email == registerDto.Email))
                throw new Exception("Email already exists");

            var role = await _context.RoleMasters
                .FirstOrDefaultAsync(r => r.Id == registerDto.RoleId && r.IsActive);
            if (role == null)
                throw new Exception("Invalid role");

            string? profileImageUrl = null;
            if (registerDto.ProfileImage != null)
            {
                profileImageUrl = await UploadProfileImage(registerDto.ProfileImage);
            }

            var user = new UserMaster
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                Password = HashPassword(registerDto.Password),
                Phone = registerDto.Phone,
                Address = registerDto.Address,
                RoleId = registerDto.RoleId,
                ProfileImage = profileImageUrl,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                RoleMaster = role
            };

            _context.UserMasters.Add(user);
            await _context.SaveChangesAsync();

            return ConvertToUserResponseDto(user);
        }

        public async Task<UserResponseDto> RegisterVendorAsync(RegisterVendorDto registerDto)
        {
 
            var vendorRole = await _context.RoleMasters
                .FirstOrDefaultAsync(r => r.Name == "Vendor" && r.IsActive);

            if (vendorRole == null)
                throw new Exception("Vendor role not found");

            if (await _context.UserMasters.AnyAsync(u => u.Email == registerDto.Email))
                throw new Exception("Email already exists");

            string? profileImageUrl = null;
            if (registerDto.ProfileImage != null)
            {
                profileImageUrl = await UploadProfileImage(registerDto.ProfileImage);
            }

            var user = new UserMaster
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                Password = HashPassword(registerDto.Password),
                Phone = registerDto.Phone,
                Address = registerDto.Address,
                RoleId = vendorRole.Id,
                ProfileImage = profileImageUrl,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                RoleMaster = vendorRole
            };

            var vendor = new VendorMaster
            {
                Name = registerDto.VendorName,
                ContactPerson = registerDto.ContactPerson,
                Phone = registerDto.VendorPhone,
                Email = registerDto.VendorEmail,
                Address = registerDto.VendorAddress,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                UserMaster = user
            };

            user.VendorMaster = vendor;

            _context.UserMasters.Add(user);
            _context.VendorMasters.Add(vendor);
            await _context.SaveChangesAsync();

            return ConvertToUserResponseDto(user);
        }

        public async Task<string> LoginAsync(LoginDto loginDto, HttpContext httpContext)
        {
            var user = await _context.UserMasters
                .Include(u => u.RoleMaster)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive);

            if (user == null || user.Password != HashPassword(loginDto.Password))
                throw new Exception("Invalid email or password");

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.RoleMaster?.Name ?? "User"),
            new Claim("UserId", user.Id.ToString())
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = loginDto.RememberMe,
                ExpiresUtc = loginDto.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(12)
            };

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return "Login successful";
        }

        public async Task<UserResponseDto> GetUserByIdAsync(int id)
        {
            var user = await _context.UserMasters
                .Include(u => u.RoleMaster)
                .Include(u => u.VendorMaster)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new Exception("User not found");

            return ConvertToUserResponseDto(user);
        }

        public async Task<List<UserResponseDto>> GetAllUsersAsync(int? roleId = null)
        {
            var query = _context.UserMasters
                .Include(u => u.RoleMaster)
                .Include(u => u.VendorMaster)
                .AsQueryable();

            if (roleId.HasValue)
            {
                query = query.Where(u => u.RoleId == roleId.Value);
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return users.Select(ConvertToUserResponseDto).ToList();
        }

        public async Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserDto updateDto)
        {
            var user = await _context.UserMasters
                .Include(u => u.RoleMaster)
                .Include(u => u.VendorMaster)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new Exception("User not found");

            if (!string.IsNullOrEmpty(updateDto.UserName))
                user.UserName = updateDto.UserName;

            if (!string.IsNullOrEmpty(updateDto.Phone))
                user.Phone = updateDto.Phone;

            if (!string.IsNullOrEmpty(updateDto.Address))
                user.Address = updateDto.Address;

            if (updateDto.IsActive.HasValue)
                user.IsActive = updateDto.IsActive.Value;

            if (updateDto.ProfileImage != null)
            {
                user.ProfileImage = await UploadProfileImage(updateDto.ProfileImage);
            }

            if (user.RoleMaster?.Name == "Vendor")
            {
                var vendor = user.VendorMaster;

                if (vendor == null)
                {
                    vendor = new VendorMaster
                    {
                        UserId = user.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.VendorMasters.Add(vendor);
                    user.VendorMaster = vendor;
                }

                if (!string.IsNullOrEmpty(updateDto.VendorName))
                    vendor.Name = updateDto.VendorName;

                if (!string.IsNullOrEmpty(updateDto.ContactPerson))
                    vendor.ContactPerson = updateDto.ContactPerson;

                if (!string.IsNullOrEmpty(updateDto.VendorPhone))
                    vendor.Phone = updateDto.VendorPhone;

                if (!string.IsNullOrEmpty(updateDto.VendorEmail))
                    vendor.Email = updateDto.VendorEmail;

                if (!string.IsNullOrEmpty(updateDto.VendorAddress))
                    vendor.Address = updateDto.VendorAddress;

                if (updateDto.IsActive.HasValue)
                    vendor.IsActive = updateDto.IsActive.Value;

                vendor.LastUpdatedAt = DateTime.UtcNow;
            }

            user.LastUpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(id);
        }

        public async Task<bool> LogoutAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return true;
        }

        public async Task<UserResponseDto> GetCurrentUserAsync(HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                throw new Exception("User not authenticated");

            return await GetUserByIdAsync(userId);
        }

        public async Task<List<UserResponseDto>> GetAllVendorsAsync(bool? isActive = null)
        {
            var query = _context.UserMasters
                .Include(u => u.RoleMaster)
                .Include(u => u.VendorMaster)
                .Where(u => u.RoleMaster.Name == "Vendor")
                .AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            var vendors = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return vendors.Select(ConvertToUserResponseDto).ToList();
        }

        public async Task<bool> DeleteVendorAsync(int userId)
        {
            var user = await _context.UserMasters
                .Include(u => u.RoleMaster)
                .Include(u => u.VendorMaster)
                .FirstOrDefaultAsync(u => u.Id == userId && u.RoleMaster.Name == "Vendor");

            if (user == null)
                throw new Exception("Vendor not found");

            user.IsActive = false;
            user.LastUpdatedAt = DateTime.UtcNow;

            if (user.VendorMaster != null)
            {
                user.VendorMaster.IsActive = false;
                user.VendorMaster.LastUpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreVendorAsync(int userId)
        {
            var user = await _context.UserMasters
                .Include(u => u.RoleMaster)
                .Include(u => u.VendorMaster)
                .FirstOrDefaultAsync(u => u.Id == userId && u.RoleMaster.Name == "Vendor");

            if (user == null)
                throw new Exception("Vendor not found");

            user.IsActive = true;
            user.LastUpdatedAt = DateTime.UtcNow;

            if (user.VendorMaster != null)
            {
                user.VendorMaster.IsActive = true;
                user.VendorMaster.LastUpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
