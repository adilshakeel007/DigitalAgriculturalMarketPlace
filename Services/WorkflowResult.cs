namespace DigitalAgriculturalMarketplace.Services;

public record WorkflowResult(bool Succeeded, string Message, int? OrderId = null)
{
    public static WorkflowResult Ok(string message, int? orderId = null) => new(true, message, orderId);
    public static WorkflowResult Fail(string message) => new(false, message);
}
