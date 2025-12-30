namespace AlphaLogistics.API.DTO
{
    public class UpdateVendorDto
    {
        public string? VendorName { get; set; }
        public string? ContactPerson { get; set; }
        public string? PAN { get; set; }
        public string? VAT { get; set; }
        public string? BankAccountNo { get; set; }
        public string? BankName { get; set; }
        public string? AccHolderName { get; set; }
        public string? PrimaryAddress { get; set; }
        public string? SecondaryAddress { get; set; }
        public string? Description { get; set; }
        public string? CustomerType { get; set; }
        public bool IsApproved { get; set; } = false;
        public bool? IsActive { get; set; }
        public int? UpdatedBy { get; set; } = null;

        public IFormFile? ProfileImage { get; set; }

        public List<IFormFile>? DocumentsToAdd { get; set; }

        public List<int>? DocumentsToDelete { get; set; }
    }
}
