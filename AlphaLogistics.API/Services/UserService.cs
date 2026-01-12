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
                CreatedAt = user.CreatedAt
            };

            return response;
        }

        public async Task<UserResponseDto> RegisterUserAsync(RegisterUserDto registerDto)
        {
           
            if (await _context.UserMasters.AnyAsync(u => u.Email == registerDto.Email))
                throw new Exception("Email already exists");

            var role = await _context.RoleMasters
                .FirstOrDefaultAsync(r => r.Id == 5 && r.IsActive);
            if (role == null)
                throw new Exception("Invalid role");

           /* string? profileImageUrl = null;
            if (registerDto.ProfileImage != null)
            {
                profileImageUrl = await UploadProfileImage(registerDto.ProfileImage);
            }*/

            var user = new UserMaster
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                Password = HashPassword(registerDto.Password),
                Phone = registerDto.Phone,
                //Address = registerDto.Address,
                RoleId = 5,
               // ProfileImage = profileImageUrl,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                RoleMaster = role
            };

            _context.UserMasters.Add(user);
            await _context.SaveChangesAsync();

            return ConvertToUserResponseDto(user);
        }

        public async Task<VendorResponseDto> RegisterVendorAsync(RegisterVendorDto registerDto, HttpContext httpContext)
        {
            
            if (!registerDto.AcceptTerms)
                throw new Exception("You must accept terms and conditions");

            if (await _context.UserMasters.AnyAsync(u => u.Email == registerDto.Email))
                throw new Exception("Email already registered");

            if (await _context.VendorMasters.AnyAsync(v => v.PAN == registerDto.PAN))
                throw new Exception("PAN number already registered");

            var vendorRole = await _context.RoleMasters
                .FirstOrDefaultAsync(r => r.Name == "Vendor" && r.IsActive);

            if (vendorRole == null)
                throw new Exception("Vendor role not found");

           
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
                Password = HashPassword(registerDto.Password),
                Phone = registerDto.Phone,
                Address = registerDto.Address,
                RoleId = vendorRole.Id,
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
            if (registerDto.Documents != null && registerDto.Documents.Any())
            {
                foreach (var documentDto in registerDto.Documents)
                {
                    var document = await UploadDocument(
                        documentDto.DocumentFile, 
                        vendor.Id,
                        documentDto.DocumentName 
                    );

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
                Role = vendorRole.Name
            };
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
                .FirstOrDefaultAsync(u => u.Id == id);

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

            if (updateDto.DocumentsToAdd != null && updateDto.DocumentsToAdd.Any())
            {
                foreach (var documentDto in updateDto.DocumentsToAdd)
                {
                   
                    if (string.IsNullOrWhiteSpace(documentDto.DocumentName))
                        throw new Exception("Document name is required for all documents");

                    if (documentDto.DocumentFile == null || documentDto.DocumentFile.Length == 0)
                        throw new Exception("Document file is required for all documents");

                    var document = await UploadDocument(
                        documentDto.DocumentFile,  
                        vendor.Id,
                        documentDto.DocumentName    
                    );

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
                .Include(v => v.Documents)
                .FirstOrDefaultAsync(v => v.Id == vendorId);

            if (vendor == null)
                throw new Exception("Vendor not found");

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
                Documents = vendor.Documents?.Select(d => new VendorDocumentDto
                {
                    DocumentId = d.Id,
                    DocumentName = d.DocumentName,
                    DocumentUrl = d.DocumentUrl,
                    UploadedAt = d.UploadedAt
                }).ToList() ?? new List<VendorDocumentDto>()
            };
        }

        public async Task<VendorListResponseDto> GetAllVendorsAsync(int pageNumber = 1,int pageSize = 10,bool? isActive = null,
            bool? isApproved = null,string? customerType = null,string? vendorName = null,string? searchQuery = null)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

            var query = _context.VendorMasters
                .Include(v => v.UserMaster)
                    .ThenInclude(u => u.RoleMaster)
                .Include(v => v.Documents)
                .Include(v => v.CreatedByUser) 
                .Include(v => v.UpdatedByUser)
                .AsQueryable();

          
            if (isActive.HasValue)
            {
                query = query.Where(v => v.IsActive == isActive.Value);
            }

            if (isApproved.HasValue)
            {
                query = query.Where(v => v.IsApproved == isApproved.Value);
            }

            if (!string.IsNullOrWhiteSpace(customerType))
            {
                query = query.Where(v => v.CustomerType == customerType);
            }

            if (!string.IsNullOrWhiteSpace(vendorName))
            {
                query = query.Where(v => v.VendorName.Contains(vendorName));
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                query = query.Where(v =>
                    v.VendorName.ToLower().Contains(searchQuery) ||
                    v.ContactPerson.ToLower().Contains(searchQuery) ||
                    v.PAN.ToLower().Contains(searchQuery) ||
                    v.UserMaster.Email.ToLower().Contains(searchQuery) ||
                    v.UserMaster.Phone.Contains(searchQuery));
            }

            var totalCount = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var vendors = await query
                .OrderByDescending(v => v.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPrevious = pageNumber > 1,
                HasNext = pageNumber < totalPages
            };
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
    }
}
