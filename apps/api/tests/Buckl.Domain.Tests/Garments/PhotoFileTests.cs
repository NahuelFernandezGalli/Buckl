using Buckl.Domain.Common;
using Buckl.Domain.Garments;

namespace Buckl.Domain.Tests.Garments;

public class PhotoFileTests
{
    [Theory]
    [InlineData("image/jpeg", "image/jpeg", "jpg")]
    [InlineData("IMAGE/JPEG", "image/jpeg", "jpg")]
    [InlineData(" image/png ", "image/png", "png")]
    [InlineData("image/webp", "image/webp", "webp")]
    public void Create_accepts_the_image_types_a_browser_can_show(string declared, string contentType, string extension)
    {
        var file = PhotoFile.Create(declared, 1024);

        Assert.Equal(contentType, file.ContentType);
        Assert.Equal(extension, file.Extension);
        Assert.Equal(1024, file.Size);
    }

    [Theory]
    [InlineData("image/gif")]
    [InlineData("image/heic")]
    [InlineData("image/svg+xml")]
    [InlineData("image/jpeg; charset=binary")]
    [InlineData("application/octet-stream")]
    [InlineData("")]
    public void Create_rejects_anything_else(string contentType)
    {
        var exception = Assert.Throws<DomainValidationException>(() => PhotoFile.Create(contentType, 1024));

        Assert.Equal(PhotoFile.Errors.UnsupportedType, exception.Code);
    }

    [Theory]
    [InlineData(0L, PhotoFile.Errors.Empty)]
    [InlineData(-1L, PhotoFile.Errors.Empty)]
    [InlineData(PhotoFile.MaxBytes + 1, PhotoFile.Errors.TooLarge)]
    public void Create_rejects_an_empty_or_oversized_photo(long size, string code)
    {
        var exception = Assert.Throws<DomainValidationException>(() => PhotoFile.Create("image/jpeg", size));

        Assert.Equal(code, exception.Code);
    }

    [Fact]
    public void Create_accepts_a_photo_of_exactly_the_maximum_size() =>
        Assert.Equal(PhotoFile.MaxBytes, PhotoFile.Create("image/jpeg", PhotoFile.MaxBytes).Size);
}
