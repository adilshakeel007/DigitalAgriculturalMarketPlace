namespace DigitalAgriculturalMarketplace.Constants
{
    public static class UserRoles
    {
        public const string Farmer = "Farmer";

        public const string Consumer = "Consumer";

        public const string Admin = "Admin";

        public const string Member = "Member";

        // Authorization policy name used by the custom session filter. It is
        // not written to legacy user records during the compatibility period.
        public const string MarketplaceMember = "MarketplaceMember";
    }
}
