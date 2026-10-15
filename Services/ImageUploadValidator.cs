using Microsoft.AspNetCore.Http;

namespace DigitalAgriculturalMarketplace.Services;

public static class ImageUploadValidator
{
    public const long MaximumBytes = 5 * 1024 * 1024;
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png" };
    private static readonly HashSet<string> ContentTypes = new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png" };

    public static async Task<(bool IsValid, string? Extension, string? Error)> ValidateAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return (true, null, null);
        if (file.Length > MaximumBytes) return (false, null, "Image size cannot exceed 5 MB.");

        var extension = Path.GetExtension(file.FileName);
        if (!Extensions.Contains(extension) || !ContentTypes.Contains(file.ContentType))
            return (false, null, "Please upload a valid JPG or PNG image.");

        var header = new byte[8];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(header.AsMemory(0, header.Length));
        var png = read >= 8 && header.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var jpeg = read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
        var signatureMatches = extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ? png : jpeg;
        return signatureMatches
            ? (true, extension.ToLowerInvariant(), null)
            : (false, null, "The uploaded file content is not a valid JPG or PNG image.");
    }
}
