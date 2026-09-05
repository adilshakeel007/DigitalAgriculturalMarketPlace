using DigitalAgriculturalMarketplace.Models;

namespace DigitalAgriculturalMarketplace.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }

        public int TotalFarmers { get; set; }

        public int TotalConsumers { get; set; }

        public int ActiveUsers { get; set; }

        public int TotalProducts { get; set; }

        public int ActiveProducts { get; set; }

        public int TotalOrders { get; set; }

        public int PendingOrders { get; set; }

        public int CompletedOrders { get; set; }

        public decimal TotalRevenue { get; set; }

        public int TotalReviews { get; set; }

        public int TotalMessages { get; set; }

        public List<Order> RecentOrders { get; set; }
            = new List<Order>();
    }
}
