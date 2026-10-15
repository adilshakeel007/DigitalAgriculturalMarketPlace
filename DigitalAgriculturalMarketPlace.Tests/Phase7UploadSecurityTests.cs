using DigitalAgriculturalMarketplace.Services;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace DigitalAgriculturalMarketPlace.Tests;

public sealed class Phase7UploadSecurityTests
{
    [Fact]
    public async Task DisguisedExecutableWithImageMetadataIsRejected()
    {
        var bytes="MZ this is not an image"u8.ToArray();
        await using var stream=new MemoryStream(bytes);
        var file=new FormFile(stream,0,bytes.Length,"ImageFile","attack.png"){Headers=new HeaderDictionary(),ContentType="image/png"};
        var result=await ImageUploadValidator.ValidateAsync(file);
        Assert.False(result.IsValid);Assert.Contains("content",result.Error!,StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidPngSignatureIsAccepted()
    {
        var bytes=new byte[]{137,80,78,71,13,10,26,10,0,0,0,0};
        await using var stream=new MemoryStream(bytes);
        var file=new FormFile(stream,0,bytes.Length,"ImageFile","crop.png"){Headers=new HeaderDictionary(),ContentType="image/png"};
        var result=await ImageUploadValidator.ValidateAsync(file);
        Assert.True(result.IsValid);Assert.Equal(".png",result.Extension);
    }

    [Fact]
    public async Task OversizedImageIsRejectedBeforeReadingContent()
    {
        await using var stream=new MemoryStream(new byte[1]);
        var file=new FormFile(stream,0,ImageUploadValidator.MaximumBytes+1,"ImageFile","crop.jpg"){Headers=new HeaderDictionary(),ContentType="image/jpeg"};
        var result=await ImageUploadValidator.ValidateAsync(file);
        Assert.False(result.IsValid);Assert.Contains("5 MB",result.Error!);
    }
}
