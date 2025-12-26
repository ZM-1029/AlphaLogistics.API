namespace AlphaLogistics.API.DTO
{
    public class VendorResponseDto : UserResponseDto
    {
        public string VendorName { get; set; }
        public string ContactPerson { get; set; }
        public string VendorEmail { get; set; }
        public string VendorPhone { get; set; }
        public string VendorAddress { get; set; }
    }
}
