using System.ComponentModel.DataAnnotations;

namespace AlphaLogistics.API.DTO
{
    public class RegisterVendorDto : RegisterUserDto
    {
        [Required]
        public string VendorName { get; set; }

        [Required]
        public string ContactPerson { get; set; }

        [Required]
        [EmailAddress]
        public string VendorEmail { get; set; }

        [Required]
        public string VendorPhone { get; set; }

        [Required]
        public string VendorAddress { get; set; }
    }
}
