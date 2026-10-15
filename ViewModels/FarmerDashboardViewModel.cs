using DigitalAgriculturalMarketplace.Models;

namespace DigitalAgriculturalMarketplace.ViewModels
{
    public class FarmerDashboardViewModel
    {
        public int TotalProducts { get; set; }

        public int ActiveProducts { get; set; }

        public int LowStockProducts { get; set; }

        public decimal TotalStockUnits { get; set; }

        public int PendingOrders { get; set; }

        public int ConfirmedOrders { get; set; }

        public int CompletedOrders { get; set; }

        public decimal TotalRevenue { get; set; }

        public int UnreadMessages { get; set; }
        public int UnreadNotifications { get; set; }

        public List<Order> RecentOrders { get; set; }
            = new List<Order>();

        public List<Product> InventoryOverview { get; set; }
            = new List<Product>();
    }
}
