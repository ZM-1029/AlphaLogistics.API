using System.ComponentModel.DataAnnotations;

namespace AlphaLogistics.API.DTO
{
    public class RegisterVendorDto
    {

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string VendorName { get; set; }

        [Required]
        public string ContactPerson { get; set; }

        [Required]
        public string PAN { get; set; }

        public string? VAT { get; set; }

        // Bank details
        [Required]
        public string BankAccountNo { get; set; }

        [Required]
        public string BankName { get; set; }

        [Required]
        public string AccHolderName { get; set; }

        [Required]
        public string PrimaryAddress { get; set; }

        public string? SecondaryAddress { get; set; }

        public string? Description { get; set; }
        public bool IsApproved { get; set; } = false;

        public string CustomerType { get; set; } = "Basic";
        public int? CreatedBy { get; set; } = null;

        public IFormFile? ProfileImage { get; set; }

        public List<string>? DocumentNames { get; set; }
        public List<IFormFile>? DocumentFiles { get; set; }

        [Required]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept terms and conditions")]
        public bool AcceptTerms { get; set; }
    }

    public class DocumentDto
    {
        [Required(ErrorMessage = "Document name is required")]
        [StringLength(200, ErrorMessage = "Document name cannot exceed 200 characters")]
        public string DocumentName { get; set; }

        [Required(ErrorMessage = "Document file is required")]
        public IFormFile DocumentFile { get; set; }
    }

}
