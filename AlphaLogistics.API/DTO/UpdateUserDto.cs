namespace AlphaLogistics.API.DTO
{
    public class UpdateUserDto
    {
        public string? UserName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public bool? IsActive { get; set; }
    }
}
