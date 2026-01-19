using System.ComponentModel.DataAnnotations;

namespace AlphaLogistics.API.DTO
{
    public class VendorApprovalRequestDto
    {
        [Required]
        public bool IsApproved { get; set; }

    }

    public class VendorApprovalResponseDto : VendorResponseDto
    {
        public string StatusMessage { get; set; }
        public DateTime ActionDate { get; set; }
        public int? ActionByUserId { get; set; }
    }
}
