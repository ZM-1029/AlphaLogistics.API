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

        // Legal details
        [Required]
        public string PAN { get; set; }

        public string? VAT { get; set; }

      
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

 
        public string CustomerType { get; set; } = "Basic"; 

        // Documents
        [Required]
        public IFormFile PANDocument { get; set; }

        public IFormFile? VATDocument { get; set; }
        public IFormFile? BankDocument { get; set; }
        public IFormFile? BusinessLicense { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public IFormFile? OtherDocument { get; set; }

        // Terms acceptance
        [Required]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept terms and conditions")]
        public bool AcceptTerms { get; set; }
    }
}
