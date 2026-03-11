namespace AlphaLogistics.API
{
    public class BankTransferDetails
    {
        public long OrderId { get; set; }
        public string BankName { get; set; }
        public string AccountHolderName { get; set; }
        public string AccountNumber { get; set; }
        public string Branch { get; set; }
        public decimal Amount { get; set; }
        public long Remarks { get; set; }
        public long UserId { get; set; }
    }
}