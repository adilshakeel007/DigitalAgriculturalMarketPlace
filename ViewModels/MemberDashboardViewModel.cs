using DigitalAgriculturalMarketplace.Models;

namespace DigitalAgriculturalMarketplace.ViewModels
{
    public class MemberDashboardViewModel
    {
        public int PurchaseCount { get; set; }
        public int ActivePurchaseCount { get; set; }
        public int ListingCount { get; set; }
        public int ActiveListingCount { get; set; }
        public int SalesCount { get; set; }
        public int PendingSalesCount { get; set; }
        public int UnreadMessages { get; set; }
        public int UnreadNotifications { get; set; }
        public SellerProfile? SellerProfile { get; set; }
        public List<Order> RecentPurchases { get; set; } = new();
        public List<Order> RecentSales { get; set; } = new();
    }
}
