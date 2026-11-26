namespace DigitalAgriculturalMarketplace.ViewModels
{
    public class AdminReportsViewModel
    {
        public int TotalUsers { get; set; }

        public int NewUsersThisMonth { get; set; }

        public int TotalOrders { get; set; }

        public int PendingOrders { get; set; }

        public int ConfirmedOrders { get; set; }

        public int CompletedOrders { get; set; }

        public int CancelledOrders { get; set; }

        public decimal TotalRevenue { get; set; }

        public decimal RevenueThisMonth { get; set; }

        public List<TopProductReportItem> TopProducts { get; set; }
            = new List<TopProductReportItem>();

        public List<TopFarmerReportItem> TopFarmers { get; set; }
            = new List<TopFarmerReportItem>();
    }


    public class TopProductReportItem
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }
            = string.Empty;

        public decimal TotalQuantitySold { get; set; }

        public decimal Revenue { get; set; }
    }


    public class TopFarmerReportItem
    {
        public int FarmerId { get; set; }

        public string FarmerName { get; set; }
            = string.Empty;

        public int CompletedOrders { get; set; }

        public decimal Revenue { get; set; }
    }
}
