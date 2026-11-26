namespace DigitalAgriculturalMarketplace.Services;

public static class QuantityRules
{
    public static bool IsValid(decimal quantity, byte decimalPlaces) =>
        quantity > 0 && quantity == decimal.Round(quantity, decimalPlaces, MidpointRounding.ToEven);

    public static string Message(string? unitCode, byte decimalPlaces) => decimalPlaces == 0
        ? $"{unitCode ?? "This unit"} only supports whole quantities."
        : $"{unitCode ?? "This unit"} supports at most {decimalPlaces} decimal places.";
}
