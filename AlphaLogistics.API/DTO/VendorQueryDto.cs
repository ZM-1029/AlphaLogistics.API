namespace AlphaLogistics.API.DTO
{
    public class VendorQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        //public bool? IsActive { get; set; }
        public bool? IsApproved { get; set; }
        public string? CustomerType { get; set; }
        public string? VendorName { get; set; }
        public string? Search { get; set; }
    }
}
