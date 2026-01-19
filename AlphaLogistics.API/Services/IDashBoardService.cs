namespace AlphaLogistics.API.Services
{
    public interface IDashBoardService
    {
        public  Task<object> GetMonthlySalesReport(int vendorId);
        public  Task<object> GraphData(int vendorId);
    }
}
