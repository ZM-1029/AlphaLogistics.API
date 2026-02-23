namespace AlphaLogistics.API.DTO
{
    public class OrderListDTO
    {
        public int? userId { get; set; }
        public int? VendorId { get; set; }
        public DateTime? from { get; set; }
        public DateTime? to { get; set; }
        public int? statusId { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 10;

    }
}
