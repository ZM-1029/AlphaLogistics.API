namespace AlphaLogistics.API.DTO
{
    public class BankAccountDetails
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string BankName { get; set; }
        public string AccountHolderName { get; set; }
        public string AccountNumber { get; set; }
        public string Branch { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "NPR";
        public string Reference { get; set; }
    }
}
