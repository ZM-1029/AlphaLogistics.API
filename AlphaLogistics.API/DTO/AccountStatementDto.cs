namespace AlphaLogistics.API.DTO
{
    public class AccountStatementRow
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DeliveryCharges { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal Commission { get; set; }
        public decimal Tax { get; set; }
        public decimal NetAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public int? PaymentTransferStatus { get; set; }
    }

    public class AccountSummary
    {
        public decimal TotalNetAmount { get; set; }
        public decimal TotalPaymentTransferPending { get; set; }
        public decimal TotalPaymentSuccessfullyTransferred { get; set; }
    }

    public class VendorStatementResult
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string VendorType { get; set; } = string.Empty;
        public decimal CommissionRate { get; set; }
        public List<AccountStatementRow> Statement { get; set; } = new();
        public AccountSummary Summary { get; set; } = new();
    }
}
