using AlphaLogistics.API.DTO;

namespace AlphaLogistics.API.Services
{
    public interface IAccountService
    {
        Task<object?> GetAccountStatement(int? vendorId, DateTime? startDate, DateTime? endDate);
        Task<byte[]> ExportAccountStatement(int? vendorId, DateTime? startDate, DateTime? endDate);
        Task<bool> UpdatePaymentTransferStatus(int orderId, int status);
    }
}
