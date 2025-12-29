using System.ComponentModel.DataAnnotations;

namespace AlphaLogistics.API.DTO
{
    public class VendorResponseDto
    {
        public int VendorId { get; set; }
        public int UserId { get; set; }

        public string VendorName { get; set; }
        public string ContactPerson { get; set; }

        // Legal details
        public string PAN { get; set; }
        public string? VAT { get; set; }

        public string BankAccountNo { get; set; }
        public string BankName { get; set; }
        public string AccHolderName { get; set; }

        public string PrimaryAddress { get; set; }
        public string? SecondaryAddress { get; set; }

        public string? Description { get; set; }


        public bool IsApproved { get; set; }
        public string CustomerType { get; set; }

        public List<VendorDocumentDto> Documents { get; set; } = new List<VendorDocumentDto>();


        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }


        public string UserName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string? ProfileImage { get; set; }
        public string Role { get; set; }
    }

    public class VendorDocumentDto
    {
        public int DocumentId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentUrl { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class AddVendorDocumentDto
    {
        [Required]
        public string DocumentName { get; set; }

        [Required]
        public IFormFile DocumentFile { get; set; }
    }

    public class ApproveVendorDto
    {
        [Required]
        public int VendorId { get; set; }

        public string? ApprovalNotes { get; set; }
        public string? CustomerType { get; set; }
    }

    public class RejectVendorDto
    {
        [Required]
        public int VendorId { get; set; }

        [Required]
        public string RejectionReason { get; set; }
    }
}
