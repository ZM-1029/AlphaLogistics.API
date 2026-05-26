using AlphaLogistics.API.DTO;
using AlphaLogistics.API.Model;
using Logistics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WALMS.API.Common;

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
        public async Task<object> ActiveRoles()
        { 
            var roles=await _context.RoleMasters.Where(x=>x.Id!=AppConstants.UserRole.Vendor).ToListAsync();

            return roles.Select(x => new { x.Id,Name=x.Name}).OrderBy(x=>x.Name);
        }
        public List<PradeshMaster> GetActivePradeshList()
        { 
            var pradeshList=_context.PradeshMasters             
                .OrderBy(p => p.Name)
                .ToList();
            return pradeshList;
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
            var CurrDir = Directory.GetCurrentDirectory();
            var uploadsFolder = Path.Combine(CurrDir, "uploads", "profiles");
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
                CreatedAt = user.CreatedAt,
                Password = user.Password != null ? EncriptorUtility.Decrypt(user.Password, false) : null
            };

            return response;
        }
        public async Task<UserResponseDto> RegisterUserAsync(RegisterUserDto registerDto)
        {
           
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
                Password = EncriptorUtility.Encrypt(registerDto.Password,false),
                Phone = registerDto.Phone,
                //Address = registerDto.Address,
                RoleId = registerDto.RoleId,
                ProfileImage = profileImageUrl,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                //RoleMaster = role
            };

            _context.UserMasters.Add(user);
            await _context.SaveChangesAsync();

            return ConvertToUserResponseDto(user);
        }
        public async Task<int> RegisterCustomerAsync(CustomerCreateDTO registerDto)
        {
            if (await _context.UserMasters.AnyAsync(u => u.Phone == registerDto.Phone))
                throw new Exception("Phone already exists");

            if (!string.IsNullOrEmpty(registerDto.Email) &&
                await _context.UserMasters.AnyAsync(u => u.Email == registerDto.Email))
                throw new Exception("Email already exists");

            // Generate password if not provided
            string plainPassword;
            bool passwordGenerated = false;
            if (string.IsNullOrWhiteSpace(registerDto.Password))
            {
                plainPassword = new GenerateRandomPassword().GetRandomAlphanumericString(10);
                passwordGenerated = true;
            }
            else
            {
                plainPassword = registerDto.Password;
            }

            var user = new UserMaster
            {
                UserName = registerDto.Name,
                Email = registerDto.Email,
                Password = EncriptorUtility.Encrypt(plainPassword, false),
                Phone = registerDto.Phone,
                PradeshId = registerDto.PradeshId,
                Address = registerDto.Address,
                RoleId = AppConstants.UserRole.Customer,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };

            _context.UserMasters.Add(user);
            await _context.SaveChangesAsync();

            // Send generated password via email — failure must not break registration
            if (passwordGenerated && !string.IsNullOrEmpty(registerDto.Email))
            {
                try
                {
                    string subject = "Your Account Has Been Created";
                    string body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                        <h2 style='color: #1565C0;'>Welcome to AlphaLogistics!</h2>
                        <p>Dear {registerDto.Name},</p>
                        <p>Your customer account has been created successfully. Below are your login credentials:</p>
                        <div style='background-color: #E3F2FD; padding: 15px; border-left: 4px solid #1565C0; margin: 20px 0;'>
                            <p><strong>Phone:</strong> {registerDto.Phone}</p>
                            <p><strong>Password:</strong> {plainPassword}</p>
                        </div>
                        <p>Please log in and change your password at your earliest convenience.</p>
                        <p><strong>The AlphaLogistics Team</strong></p>
                        <hr style='margin: 20px 0; border: none; border-top: 1px solid #ddd;'>
                        <p style='font-size: 12px; color: #666;'>This is an automated message. Please do not reply to this email.</p>
                    </body>
                    </html>";

                    await EmailService.SendEmailAsync(registerDto.Email, subject, body);
                }
                catch (Exception emailEx)
                {
                    // Log warning but do not fail registration if email delivery fails
                    Console.Error.WriteLine($"RegisterCustomer: email send failed for {registerDto.Email} — {emailEx.Message}");
                }
            }

            return user.Id;
        }
        public async Task<bool> UpdateCustomerAsync(CustomerCreateDTO registerDto)
        {

            if (await _context.UserMasters.AnyAsync(u => u.Phone == registerDto.Phone && u.Id != registerDto.Id))
                throw new Exception("Phone already exists");

            var existingCustomer= await _context.UserMasters.FirstOrDefaultAsync(u => u.Id == registerDto.Id && u.RoleId==AppConstants.UserRole.Customer);

            if (existingCustomer == null) return false;
            existingCustomer.Address=registerDto.Address;
            existingCustomer.Phone=registerDto.Phone;
            existingCustomer.PradeshId= registerDto.PradeshId;
            existingCustomer.Email=registerDto.Email;
            existingCustomer.Password= EncriptorUtility.Encrypt(registerDto.Password, false);
            existingCustomer.IsActive = registerDto.IsActive;

           
            _context.UserMasters.Update(existingCustomer);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<VendorResponseDto> RegisterVendorAsync(RegisterVendorDto registerDto, HttpContext httpContext)
        {
            
            if (!registerDto.AcceptTerms)
                throw new Exception("You must accept terms and conditions");

            if (await _context.UserMasters.AnyAsync(u => u.Email == registerDto.Email))
                throw new Exception("Email already registered");

            if (await _context.VendorMasters.AnyAsync(v => v.PAN == registerDto.PAN))
                throw new Exception("PAN number already registered");

            /*var vendorRole = await _context.RoleMasters
                .FirstOrDefaultAsync(r => r.Name == "Vendor" && r.IsActive);

            if (vendorRole == null)
                throw new Exception("Vendor role not found");*/

           
            int? createdByUserId = null;
            bool isAdminOrSuperAdmin = false;

            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                   
                    var currentUser = await _context.UserMasters
                        .Include(u => u.RoleMaster)
                        .FirstOrDefaultAsync(u => u.Id == currentUserId);

                    if (currentUser != null && currentUser.RoleMaster != null)
                    {
                        var roleName = currentUser.RoleMaster.Name;
                        if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                            roleName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                        {
                            createdByUserId = currentUserId;
                            isAdminOrSuperAdmin = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking user role: {ex.Message}");
            }

            string? profileImageUrl = null;
            if (registerDto.ProfileImage != null)
            {
                profileImageUrl = await UploadProfileImage(registerDto.ProfileImage);
            }

            // Create user account
            var user = new UserMaster
            {
                UserName = registerDto.VendorName,
                Email = registerDto.Email,
                Password = EncriptorUtility.Encrypt(registerDto.Password,false),
                Phone = registerDto.Phone,
                Address = registerDto.Address,
                RoleId = AppConstants.UserRole.Vendor,
                ProfileImage = profileImageUrl,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            try
            {
                _context.UserMasters.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {

                if (dbEx.InnerException != null)
                {
                    throw new Exception($"{dbEx.InnerException}");
                }
            }

            // Create vendor
            var vendor = new VendorMaster
            {
                UserId = user.Id,
                CreatedBy = createdByUserId, 
                UpdatedBy = createdByUserId, 
                VendorName = registerDto.VendorName,
                ContactPerson = registerDto.ContactPerson,
                PAN = registerDto.PAN,
                VAT = registerDto.VAT,
                BankAccountNo = registerDto.BankAccountNo,
                BankName = registerDto.BankName,
                AccHolderName = registerDto.AccHolderName,
                PrimaryAddress = registerDto.PrimaryAddress,
                SecondaryAddress = registerDto.SecondaryAddress,
                Description = registerDto.Description,
                IsApproved = isAdminOrSuperAdmin, 
                CustomerType = registerDto.CustomerType,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow,
                IsActive = isAdminOrSuperAdmin 
            };

            try
            {
                _context.VendorMasters.Add(vendor);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {

                if (dbEx.InnerException != null)
                {
                    throw new Exception($"{dbEx.InnerException}");
                }
            }

            var documents = new List<DocumentMaster>();
            if (registerDto.DocumentFiles != null && registerDto.DocumentFiles.Any())
            {
                var names = registerDto.DocumentNames ?? new List<string>();
                for (int i = 0; i < registerDto.DocumentFiles.Count; i++)
                {
                    var docName = i < names.Count ? names[i] : $"Document {i + 1}";
                    var document = await UploadDocument(registerDto.DocumentFiles[i], vendor.Id, docName);
                    if (document != null)
                        documents.Add(document);
                }

                if (documents.Any())
                {
                    _context.DocumentMasters.AddRange(documents);
                    await _context.SaveChangesAsync();
                }
            }

            // Prepare response
            var vendorDocuments = documents.Select(d => new VendorDocumentDto
            {
                DocumentId = d.Id,
                DocumentName = d.DocumentName,
                DocumentUrl = d.DocumentUrl,
                UploadedAt = d.UploadedAt
            }).ToList();

            return new VendorResponseDto
            {
                VendorId = vendor.Id,
                UserId = user.Id,
                VendorName = vendor.VendorName,
                ContactPerson = vendor.ContactPerson,
                PAN = vendor.PAN,
                VAT = vendor.VAT,
                BankAccountNo = vendor.BankAccountNo,
                BankName = vendor.BankName,
                AccHolderName = vendor.AccHolderName,
                PrimaryAddress = vendor.PrimaryAddress,
                SecondaryAddress = vendor.SecondaryAddress,
                Description = vendor.Description,
                IsApproved = vendor.IsApproved,
                CustomerType = vendor.CustomerType,
                CreatedBy = vendor.CreatedBy,
                UpdatedBy = vendor.UpdatedBy,
                Documents = vendorDocuments,
                CreatedAt = vendor.CreatedAt,
                IsActive = vendor.IsActive,
                UserName = user.UserName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                ProfileImage = user.ProfileImage,
               // Role = vendorRole.Name
            };
        }

        public async Task<string> LoginAsync(LoginDto loginDto, HttpContext httpContext)
        {
            var user = await _context.UserMasters
                .Include(u => u.RoleMaster)
                .Include(u => u.VendorMaster)
                .FirstOrDefaultAsync(u => u.Email!=null &&  u.Email.Trim().ToLower() == loginDto.Email.Trim().ToLower() && u.IsActive);

            if (user != null && user.VendorMaster?.IsApproved == false)
            {
                throw new Exception("Not an approved Vendor");
            }

            var encryptedPassword = EncriptorUtility.Encrypt(loginDto.Password,false);
            if (user == null || user.Password != encryptedPassword)
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

        public async Task<int> UserCount( int? roleId)
        {
            var query = await _context.UserMasters.ToListAsync();
            query = query.Where(u => /*u.RoleId != AppConstants.UserRole.Customer &&*/ u.RoleId != AppConstants.UserRole.Vendor).ToList();

            if (roleId != null && roleId > 0)
                query = query.Where(x => x.RoleId == roleId).ToList();
            return query.Count();
        }

        public async Task<List<UserResponseDto>> GetAllUsersAsync(int? roleId, int page, int pageSize)
        {
            var query = _context.UserMasters
                .Include(u => u.RoleMaster)
                .Include(u => u.VendorMaster)
                .AsQueryable();

            query = query.Where(u => /*u.RoleId != AppConstants.UserRole.Customer &&*/ u.RoleId != AppConstants.UserRole.Vendor);

            if(roleId !=null && roleId>0 )
                query=query.Where(x=>x.RoleId==roleId);

            query =  query
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize);                     

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return users.Select(ConvertToUserResponseDto).ToList();
        }
        public async Task<List<dynamic>> GetAllCustomerAsync()
        {
           var users = await _context.UserMasters
                .Include(u => u.RoleMaster)
                .Where(u => u.RoleId == AppConstants.UserRole.Customer)
                .OrderBy(u => u.UserName)
                .ToListAsync();

            var pradesh=_context.PradeshMasters.ToList();

            var response = users.Select(x => (dynamic)new {
            x.Id,
            x.UserName,
            x.Email,
            x.Phone,
            x.Address,
             PradeshName=pradesh.FirstOrDefault(p=>p.Id==x.PradeshId)?.Name
            }).ToList();

            return response;
        }

        public async Task<dynamic> GetCustomerByIdAsync(int customerId)
        {
            var users =  _context.UserMasters
                 .FirstOrDefault(u => u.Id == customerId);
                
            var pradesh = _context.PradeshMasters.ToList();

            var response = (dynamic)new
            {
                users.Id,
                users.UserName,
                users.Email,
                users.Phone,
                users.Address,
                PradeshName = pradesh.FirstOrDefault(p => p.Id == users.PradeshId)?.Name
            };

            return response;
        }

        public async Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserDto updateDto)
        {
            var user = await _context.UserMasters
                .Include(u => u.RoleMaster)
                .FirstOrDefaultAsync(u => u.Id == id && /*u.RoleId!=AppConstants.UserRole.Customer && */u.RoleId != AppConstants.UserRole.Vendor);

            if (user == null)
                throw new Exception("User not found");

            // Update user details if provided
            if (!string.IsNullOrEmpty(updateDto.UserName))
                user.UserName = updateDto.UserName;

            if (!string.IsNullOrEmpty(updateDto.Email))
                user.Email = updateDto.Email;

            if (!string.IsNullOrEmpty(updateDto.Phone))
                user.Phone = updateDto.Phone;

            if (!string.IsNullOrEmpty(updateDto.Address))
                user.Address = updateDto.Address;

            if (updateDto.IsActive.HasValue)
                user.IsActive = updateDto.IsActive.Value;

            if (!string.IsNullOrEmpty(updateDto.Password))
                user.Password = EncriptorUtility.Encrypt(updateDto.Password, false);
            //Password = EncriptorUtility.Encrypt(registerDto.Password, false),

            // Update profile image if provided
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

        public async Task<VendorResponseDto> UpdateVendorAsync(int vendorId, UpdateVendorDto updateDto, HttpContext httpContext)
        {
            var vendor = await _context.VendorMasters
                .Include(v => v.UserMaster)
                .Include(v => v.Documents)
                .FirstOrDefaultAsync(v => v.Id == vendorId);

            if (vendor == null)
                throw new Exception("Vendor not found");

            int? updatedByUserId = null;
            bool isAdminOrSuperAdmin = false;

            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    var currentUser = await _context.UserMasters
                        .Include(u => u.RoleMaster)
                        .FirstOrDefaultAsync(u => u.Id == currentUserId);

                    if (currentUser != null && currentUser.RoleMaster != null)
                    {
                        var roleName = currentUser.RoleMaster.Name;
                        if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                            roleName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                        {
                            updatedByUserId = currentUserId;
                            isAdminOrSuperAdmin = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking user role: {ex.Message}");
            }

            if (!string.IsNullOrEmpty(updateDto.VendorName))
                vendor.VendorName = updateDto.VendorName;

            if (!string.IsNullOrEmpty(updateDto.ContactPerson))
                vendor.ContactPerson = updateDto.ContactPerson;

            if (!string.IsNullOrEmpty(updateDto.PAN))
            {
              
                if (await _context.VendorMasters.AnyAsync(v => v.PAN == updateDto.PAN && v.Id != vendorId))
                    throw new Exception("PAN number already registered");

                vendor.PAN = updateDto.PAN;
            }

            if (!string.IsNullOrEmpty(updateDto.VAT))
                vendor.VAT = updateDto.VAT;

            if (!string.IsNullOrEmpty(updateDto.BankAccountNo))
                vendor.BankAccountNo = updateDto.BankAccountNo;

            if (!string.IsNullOrEmpty(updateDto.BankName))
                vendor.BankName = updateDto.BankName;

            if (!string.IsNullOrEmpty(updateDto.AccHolderName))
                vendor.AccHolderName = updateDto.AccHolderName;

            if (!string.IsNullOrEmpty(updateDto.PrimaryAddress))
                vendor.PrimaryAddress = updateDto.PrimaryAddress;

            if (!string.IsNullOrEmpty(updateDto.SecondaryAddress))
                vendor.SecondaryAddress = updateDto.SecondaryAddress;

            if (!string.IsNullOrEmpty(updateDto.Description))
                vendor.Description = updateDto.Description;

            if (!string.IsNullOrEmpty(updateDto.CustomerType))
                vendor.CustomerType = updateDto.CustomerType;

            if (updateDto.IsActive.HasValue)
                vendor.IsActive = updateDto.IsActive.Value;


            if (updateDto.IsApproved)
            {
                if (isAdminOrSuperAdmin)
                {
                    vendor.IsApproved = updateDto.IsApproved;


                    if (vendor.IsApproved && !vendor.IsActive)
                        vendor.IsActive = true;
                }
                else
                {
                    throw new UnauthorizedAccessException("Only Admin or SuperAdmin can update approval status");
                }
            }

            vendor.LastUpdatedAt = DateTime.UtcNow;

            if (isAdminOrSuperAdmin)
                vendor.UpdatedBy = updatedByUserId;

            var currDirectory = Directory.GetCurrentDirectory();
            if (updateDto.ProfileImage != null)
            {
               
                if (!string.IsNullOrEmpty(vendor.UserMaster.ProfileImage))
                {
                    var oldImagePath = Path.Combine(currDirectory, vendor.UserMaster.ProfileImage.TrimStart('/'));
                    if (File.Exists(oldImagePath))
                    {
                        File.Delete(oldImagePath);
                    }
                }

                vendor.UserMaster.ProfileImage = await UploadProfileImage(updateDto.ProfileImage);
            }

            var newDocuments = new List<DocumentMaster>();

            if (updateDto.DocumentFiles != null && updateDto.DocumentFiles.Any())
            {
                var names = updateDto.DocumentNames ?? new List<string>();
                for (int i = 0; i < updateDto.DocumentFiles.Count; i++)
                {
                    var file = updateDto.DocumentFiles[i];
                    if (file == null || file.Length == 0)
                        continue;

                    var docName = i < names.Count ? names[i] : $"Document {i + 1}";
                    var document = await UploadDocument(file, vendor.Id, docName);
                    if (document != null)
                        newDocuments.Add(document);
                }

                if (newDocuments.Any())
                {
                    _context.DocumentMasters.AddRange(newDocuments);
                }
            }

            if (updateDto.DocumentsToDelete != null && updateDto.DocumentsToDelete.Any())
            {
                var docsToDelete = await _context.DocumentMasters
                    .Where(d => updateDto.DocumentsToDelete.Contains(d.Id) && d.VendorId == vendorId)
                    .ToListAsync();

               
                foreach (var doc in docsToDelete)
                {
                    if (!string.IsNullOrEmpty(doc.DocumentUrl))
                    {
                        var filePath = Path.Combine("wwwroot", doc.DocumentUrl.TrimStart('/'));
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
                }

                _context.DocumentMasters.RemoveRange(docsToDelete);
            }

            await _context.SaveChangesAsync();

            return await GetVendorByIdAsync(vendorId);
        }

        public async Task<VendorResponseDto> ApproveOrRejectVendorAsync(int vendorId, bool isApproved, HttpContext httpContext,string? reason)
        {
            // Get the vendor with user information
            var vendor = await _context.VendorMasters
                .Include(v => v.UserMaster)
                .FirstOrDefaultAsync(v => v.Id == vendorId);

            if (vendor == null)
                throw new Exception("Vendor not found");

            int? updatedByUserId = null;
            bool isAdminOrSuperAdmin = false;

            try
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    var currentUser = await _context.UserMasters
                        .Include(u => u.RoleMaster)
                        .FirstOrDefaultAsync(u => u.Id == currentUserId);

                    if (currentUser != null && currentUser.RoleMaster != null)
                    {
                        var roleName = currentUser.RoleMaster.Name;
                        if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                            roleName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                        {
                            updatedByUserId = currentUserId;
                            isAdminOrSuperAdmin = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking user role: {ex.Message}");
            }

            if (!isAdminOrSuperAdmin)
                throw new UnauthorizedAccessException("Only Admin or SuperAdmin can approve/reject vendors");

            vendor.IsApproved = isApproved;
            vendor.Reason = reason;
            if (isApproved && !vendor.IsActive)
                vendor.IsActive = true;

            if (!isApproved)
            {
                vendor.IsActive = false; 
            }

            vendor.LastUpdatedAt = DateTime.UtcNow;
            vendor.UpdatedBy = updatedByUserId;

            await _context.SaveChangesAsync();

            var user=_context.UserMasters.FirstOrDefault(u=>u.Id==vendor.UserId);

            if (isApproved)
            {
                string approvalSubject = "Congratulations! Your Vendor Application Has Been Approved";
                string approvalBody = @$"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                        <h2 style='color: #2E7D32;'>Vendor Application Approved!</h2>
                        <p>Dear {user?.UserName ?? "Vendor"},</p>
                        <p>We are pleased to inform you that your vendor application has been <strong>approved</strong>.</p>
            
                        <div style='background-color: #E8F5E9; padding: 15px; border-left: 4px solid #2E7D32; margin: 20px 0;'>
                            <h3>Next Steps:</h3>
                            <ul>
                                <li>You can now log in to your vendor portal</li>
                                <li>Access your vendor dashboard to manage your products/services</li>
                                <li>Review our vendor guidelines and policies</li>
                            </ul>
                        </div>
            
                        <p>If you have any questions or need assistance, please don't hesitate to contact our support team.</p>
            
                        <p>Welcome aboard!</p>
                        <p><strong>The Vendor Management Team</strong></p>
            
                        <hr style='margin: 20px 0; border: none; border-top: 1px solid #ddd;'>
                        <p style='font-size: 12px; color: #666;'>
                            This is an automated message. Please do not reply to this email.
                        </p>
                    </body>
                    </html>";

                await EmailService.SendEmailAsync(user?.Email, approvalSubject, approvalBody);
            }
            else
            {
                string rejectionSubject = "Update on Your Vendor Application";
                string rejectionBody = @$"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                        <h2 style='color: #C62828;'>Vendor Application Status Update</h2>
                        <p>Dear {user?.UserName ?? "Vendor"},</p>
                        <p>Thank you for your interest in becoming a vendor with us. After careful review, we regret to inform you that your application has <strong>not been approved</strong> at this time.</p>
            
                        <div style='background-color: #FFEBEE; padding: 15px; border-left: 4px solid #C62828; margin: 20px 0;'>
                            <h3>Possible Reasons:</h3>
                            <ul>
                                <li>Incomplete documentation or information</li>
                                <li>Currently not accepting vendors in your category</li>
                                <li>Business requirements not fully met</li>
                            </ul>
                        </div>
            
                        <p><strong>Note:</strong> This decision is based on our current business needs and requirements. You are welcome to reapply in the future when you feel the circumstances have changed.</p>
            
                        <p>If you would like feedback on your application or have questions about our vendor requirements, please contact our vendor relations team.</p>
            
                        <p>We appreciate your interest and wish you success in your business endeavors.</p>
            
                        <p><strong>The Vendor Management Team</strong></p>
            
                        <hr style='margin: 20px 0; border: none; border-top: 1px solid #ddd;'>
                        <p style='font-size: 12px; color: #666;'>
                            This is an automated message. Please do not reply to this email.
                        </p>
                    </body>
                    </html>";

                await EmailService.SendEmailAsync(user?.Email, rejectionSubject, rejectionBody);
            }


            return await GetVendorByIdAsync(vendorId);
        }

        public async Task<List<VendorDocumentDto>> GetVendorDocumentsAsync(int vendorId)
        {
            var documents = await _context.DocumentMasters
                .Where(d => d.VendorId == vendorId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            return documents.Select(d => new VendorDocumentDto
            {
                DocumentId = d.Id,
                DocumentName = d.DocumentName,
                DocumentUrl = d.DocumentUrl,
                UploadedAt = d.UploadedAt
            }).ToList();
        }

        public async Task<VendorResponseDto> GetVendorByIdAsync(int vendorId)
        {
            var vendor = await _context.VendorMasters
                .Include(v => v.UserMaster)
                    .ThenInclude(u => u.RoleMaster)
                .FirstOrDefaultAsync(v => v.Id == vendorId);

            if (vendor == null)
                throw new Exception("Vendor not found");

            var documents = await _context.DocumentMasters
                .Where(d => d.VendorId == vendorId)
                .ToListAsync();

            return new VendorResponseDto
            {
                VendorId = vendor.Id,
                UserId = vendor.UserId,

                // Vendor details
                VendorName = vendor.VendorName,
                ContactPerson = vendor.ContactPerson,
                PAN = vendor.PAN,
                VAT = vendor.VAT,
                BankAccountNo = vendor.BankAccountNo,
                BankName = vendor.BankName,
                AccHolderName = vendor.AccHolderName,
                PrimaryAddress = vendor.PrimaryAddress,
                SecondaryAddress = vendor.SecondaryAddress,
                Description = vendor.Description,
                IsApproved = vendor.IsApproved,
                CustomerType = vendor.CustomerType,
                CreatedBy = vendor.CreatedBy,
                UpdatedBy = vendor.UpdatedBy,

                // Timestamps
                CreatedAt = vendor.CreatedAt,
                IsActive = vendor.IsActive,

                // User info
                UserName = vendor.UserMaster.UserName,
                Email = vendor.UserMaster.Email,
                Phone = vendor.UserMaster.Phone,
                Address = vendor.UserMaster.Address,
                ProfileImage = vendor.UserMaster.ProfileImage,
                Role = vendor.UserMaster.RoleMaster?.Name ?? "Vendor",

                // Documents
                Documents = documents.Select(d => new VendorDocumentDto
                {
                    DocumentId = d.Id,
                    DocumentName = d.DocumentName,
                    DocumentUrl = d.DocumentUrl,
                    UploadedAt = d.UploadedAt
                }).ToList()
            };
        }

        public async Task<VendorListResponseDto> GetAllVendorsAsync(VendorQueryDto dto)
        {
            dto.Page = dto.Page < 1 ? 1 : dto.Page;
            dto.PageSize = dto.PageSize < 1 ? 10 : (dto.PageSize > 100 ? 100 : dto.PageSize);

            var query = _context.VendorMasters
                .Include(v => v.UserMaster)
                    .ThenInclude(u => u.RoleMaster)
                .Include(v => v.Documents)
                .Include(v => v.CreatedByUser) 
                .Include(v => v.UpdatedByUser).Where(x=>x.UserMaster.RoleId==AppConstants.UserRole.Vendor)
                .AsQueryable();

          
            /*if (dto.IsActive.HasValue)
            {
                query = query.Where(v => v.IsActive == dto.IsActive.Value);
            }*/

            if (dto.IsApproved.HasValue)
            {
                query = query.Where(v => v.IsApproved == dto.IsApproved.Value);
            }

            if (!string.IsNullOrWhiteSpace(dto.CustomerType))
            {
                query = query.Where(v => v.CustomerType == dto.CustomerType);
            }

            if (!string.IsNullOrWhiteSpace(dto.VendorName))
            {
                query = query.Where(v => v.VendorName.Contains(dto.VendorName));
            }

            if (!string.IsNullOrWhiteSpace(dto.Search))
            {
                dto.Search = dto.Search.ToLower();
                query = query.Where(v =>
                    v.VendorName.ToLower().Contains(dto.Search) ||
                    v.ContactPerson.ToLower().Contains(dto.Search) ||
                    v.PAN.ToLower().Contains(dto.Search) ||
                    v.UserMaster.Email.ToLower().Contains(dto.Search) ||
                    v.UserMaster.Phone.Contains(dto.Search));
            }

            var totalCount = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)dto.PageSize);

            var vendors = await query
                .OrderByDescending(v => v.CreatedAt)
                .Skip((dto.Page - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync();

            var vendorDtos = vendors.Select(v => new VendorResponseDto
            {
                VendorId = v.Id,
                UserId = v.UserId,
                VendorName = v.VendorName,
                ContactPerson = v.ContactPerson,
                PAN = v.PAN,
                VAT = v.VAT,
                BankAccountNo = v.BankAccountNo,
                BankName = v.BankName,
                AccHolderName = v.AccHolderName,
                PrimaryAddress = v.PrimaryAddress,
                SecondaryAddress = v.SecondaryAddress,
                Description = v.Description,
                IsApproved = v.IsApproved,
                CustomerType = v.CustomerType,
                CreatedBy = v.CreatedBy,
                UpdatedBy = v.UpdatedBy,
                CreatedAt = v.CreatedAt,
                IsActive = v.IsActive,

         
                UserName = v.UserMaster.UserName,
                Email = v.UserMaster.Email,
                Phone = v.UserMaster.Phone,
                Address = v.UserMaster.Address,
                ProfileImage = v.UserMaster.ProfileImage,
                Role = v.UserMaster.RoleMaster?.Name ?? "Vendor",

               
                Documents = v.Documents?.Select(d => new VendorDocumentDto
                {
                    DocumentId = d.Id,
                    DocumentName = d.DocumentName,
                    DocumentUrl = d.DocumentUrl,
                    UploadedAt = d.UploadedAt
                }).ToList() ?? new List<VendorDocumentDto>()
            }).ToList();

          
            return new VendorListResponseDto
            {
                Vendors = vendorDtos,
                CurrentPage = dto.Page,
                PageSize = dto.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPrevious = dto.Page > 1,
                HasNext = dto.Page < totalPages
            };
        }
        public async Task<dynamic> GetActiveVendor()
        {
            var vendorList = await _context.VendorMasters.Where(x => x.IsActive && x.IsApproved).ToListAsync();
            var result = vendorList.Select(x => new { Id = x.Id, Name = x.VendorName });
            return result;
        }
        public async Task<dynamic> GetActiveUsers()
        {
            var userList = await _context.UserMasters.Where(x => x.IsActive && x.RoleId==AppConstants.UserRole.Customer).ToListAsync();
            var result = userList.Select(x => new { Id = x.Id, Name = x.UserName });
            return result;
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

        /*        public async Task<List<UserResponseDto>> GetAllVendorsAsync(bool? isActive = null)
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
*/
        public async Task<bool> DeleteVendorAsync(int vendorId)
        {
            var vendor = await _context.VendorMasters
                .Include(v => v.UserMaster)
                .FirstOrDefaultAsync(v => v.Id == vendorId);

            if (vendor == null)
                throw new Exception("Vendor not found");

           
            vendor.IsActive = false;
            vendor.LastUpdatedAt = DateTime.UtcNow;

            vendor.UserMaster.IsActive = false;
            vendor.UserMaster.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreVendorAsync(int vendorId)
        {
            var vendor = await _context.VendorMasters
                .Include(v => v.UserMaster)
                .FirstOrDefaultAsync(v => v.Id == vendorId);

            if (vendor == null)
                throw new Exception("Vendor not found");

            vendor.IsActive = true;
            vendor.LastUpdatedAt = DateTime.UtcNow;

            vendor.UserMaster.IsActive = true;
            vendor.UserMaster.LastUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }


        // Add vendor document
        public async Task<VendorDocumentDto> AddVendorDocumentAsync(int vendorId, AddVendorDocumentDto addDocumentDto)
        {
          
            var vendor = await _context.VendorMasters
                .FirstOrDefaultAsync(v => v.Id == vendorId);

            if (vendor == null)
                throw new Exception("Vendor not found");

            if (!vendor.IsActive || !vendor.IsApproved)
                throw new Exception("Vendor is not active or approved");

            var documentName = !string.IsNullOrEmpty(addDocumentDto.DocumentName)
                ? addDocumentDto.DocumentName
                : addDocumentDto.DocumentFile.FileName;

            var document = await UploadDocument(
                addDocumentDto.DocumentFile,
                vendorId,
                documentName
            );

            if (document == null)
                throw new Exception("Failed to upload document");

            _context.DocumentMasters.Add(document);
            await _context.SaveChangesAsync();

            return new VendorDocumentDto
            {
                DocumentId = document.Id,
                DocumentName = document.DocumentName,
                DocumentUrl = document.DocumentUrl,
                UploadedAt = document.UploadedAt
            };
        }

        public async Task<bool> DeleteVendorDocumentAsync(int documentId)
        {
            var document = await _context.DocumentMasters
                .Include(d => d.VendorMaster)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                throw new Exception("Document not found");

            if (!document.VendorMaster.IsActive || !document.VendorMaster.IsApproved)
                throw new Exception("Cannot delete document from inactive or unapproved vendor");

            // Delete physical file
            DeleteFile(document.DocumentUrl);

            // Delete database record
            _context.DocumentMasters.Remove(document);
            await _context.SaveChangesAsync();

            return true;
        }

        private void DeleteFile(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return;

            try
            {
                
                var fileName = Path.GetFileName(fileUrl);
                var currDirectory = Directory.GetCurrentDirectory();
                
                string folder = "uploads";
                if (fileUrl.Contains("/profiles/"))
                    folder = "uploads/profiles";
                else if (fileUrl.Contains("/documents/"))
                    folder = "uploads/documents";
                else if (fileUrl.Contains("/products/"))
                    folder = "uploads/products";

                var filePath = Path.Combine(currDirectory, folder, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
            }
        }

        private async Task<DocumentMaster> UploadDocument(IFormFile file, int vendorId, string documentName)
        {
            var documentUrl = await UploadFile(file, "documents");

            if (string.IsNullOrEmpty(documentUrl))
                return null;

            return new DocumentMaster
            {
                VendorId = vendorId,
                DocumentName = documentName,
                DocumentUrl = documentUrl,
                UploadedAt = DateTime.UtcNow
            };
        }
        private async Task<string?> UploadFile(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return null;
            var currDirectory = Directory.GetCurrentDirectory();
            var uploadsFolder = Path.Combine(currDirectory, "uploads", folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/{folder}/{uniqueFileName}";
        }

        public async Task<VendorMaster?> GetVendorByUserId(int userId)
        {
            return await _context.VendorMasters.FirstOrDefaultAsync(v => v.UserId == userId);
        }
    }
}
