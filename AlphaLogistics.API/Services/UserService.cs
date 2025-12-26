using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Model;

namespace AlphaLogistics.API.Services
{
    public class UserService : IUserService
    {
        private readonly AlphaLogisticsContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public UserService(
            AlphaLogisticsContext context,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _configuration = configuration;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private async Task<string?> UploadProfileImage(IFormFile profileImage)
        {
            if (profileImage == null || profileImage.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + profileImage.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(fileStream);
            }

            return $"/uploads/profiles/{uniqueFileName}";
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
                IsActive = true
            };

            _context.UserMasters.Add(user);
            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = role.Name,
                ProfileImage = user.ProfileImage,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<VendorResponseDto> RegisterVendorAsync(RegisterVendorDto registerDto)
        {
            var vendorRole = await _context.RoleMasters
                .FirstOrDefaultAsync(r => r.Name == "Vendor" && r.IsActive);

            if (vendorRole == null)
                throw new Exception("Vendor role not found");

            var userRegisterDto = new RegisterUserDto
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                Password = registerDto.Password,
                Phone = registerDto.Phone,
                Address = registerDto.Address,
                RoleId = vendorRole.Id,
                ProfileImage = registerDto.ProfileImage
            };

            var user = await RegisterUserAsync(userRegisterDto);

            var vendor = new VendorMaster
            {
                UserId = user.Id,
                Name = registerDto.VendorName,
                ContactPerson = registerDto.ContactPerson,
                Phone = registerDto.VendorPhone,
                Email = registerDto.VendorEmail,
                Address = registerDto.VendorAddress,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.VendorMasters.Add(vendor);
            await _context.SaveChangesAsync();

            var vendorResponse = new VendorResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                ProfileImage = user.ProfileImage,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                VendorName = vendor.Name,
                ContactPerson = vendor.ContactPerson,
                VendorEmail = vendor.Email,
                VendorPhone = vendor.Phone,
                VendorAddress = vendor.Address
            };

            return vendorResponse;
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

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

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

        public async Task<UserResponseDto> GetCurrentUserAsync(HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                throw new Exception("User not authenticated");

            return await GetUserByIdAsync(userId);
        }

        public async Task<UserResponseDto> GetUserByIdAsync(int id)
        {
            var user = await _context.UserMasters
                .Include(u => u.RoleMaster)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new Exception("User not found");

            return new UserResponseDto
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
        }

        public async Task<List<UserResponseDto>> GetAllUsersAsync(int? roleId = null)
        {
            var query = _context.UserMasters
                .Include(u => u.RoleMaster)
                .AsQueryable();

            if (roleId.HasValue)
            {
                query = query.Where(u => u.RoleId == roleId.Value);
            }

            var users = await query.ToListAsync();

            return users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Phone = u.Phone,
                Address = u.Address,
                Role = u.RoleMaster?.Name ?? "User",
                ProfileImage = u.ProfileImage,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToList();
        }

        public async Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserDto updateDto)
        {
            var user = await _context.UserMasters
                .Include(u => u.RoleMaster)
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

            user.LastUpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new UserResponseDto
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
        }

        public async Task<bool> LogoutAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return true;
        }
    }
}
