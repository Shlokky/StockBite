using StockBite.Models;


namespace StockBite.Services
{
    public interface IRoleContext
    {
        UserRole CurrentRole { get; }
        void SetRole(UserRole role);

        int? CurrentVendorId { get; }
        void SetVendorId(int? vendorId);

        int? CurrentConsumerId { get; }
        void SetConsumerId(int? consumerId);

        bool IsAuthenticated { get; }
        void SetAuthenticated(bool isAuthenticated);
    }
}
