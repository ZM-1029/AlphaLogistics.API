namespace AlphaLogistics.API.DTO
{
    public class CustomerCreateDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int PradeshId { get; set; }
        public bool IsActive { get; set; } = true;
        public string Password { get; set; }
    }
}
