using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Domain.Users;

namespace Buckl.Domain.Tests.Garments;

public class PhotoKeyTests
{
    private static readonly UserId Owner = new(Guid.Parse("11111111-2222-3333-4444-555555555555"));

    [Fact]
    public void PrefixFor_is_users_slash_id_slash()
    {
        Assert.Equal("users/11111111-2222-3333-4444-555555555555/", PhotoKey.PrefixFor(Owner));
    }

    [Fact]
    public void Create_accepts_a_key_under_the_owner_prefix()
    {
        var key = PhotoKey.Create($"users/{Owner.Value}/garments/abc.jpg", Owner);

        Assert.Equal($"users/{Owner.Value}/garments/abc.jpg", key.Value);
        Assert.Equal(Owner, key.OwnerId);
    }

    [Fact]
    public void Create_rejects_a_key_under_another_users_prefix()
    {
        var other = UserId.New();

        var exception = Assert.Throws<DomainValidationException>(
            () => PhotoKey.Create($"users/{other.Value}/garments/abc.jpg", Owner));

        Assert.Equal(PhotoKey.Errors.OutsideOwnerPrefix, exception.Code);
    }

    [Theory]
    [InlineData("garments/abc.jpg")]
    [InlineData("Users/11111111-2222-3333-4444-555555555555/abc.jpg")]
    public void Create_rejects_a_key_without_the_owner_prefix(string value)
    {
        var exception = Assert.Throws<DomainValidationException>(() => PhotoKey.Create(value, Owner));

        Assert.Equal(PhotoKey.Errors.OutsideOwnerPrefix, exception.Code);
    }

    [Fact]
    public void Create_rejects_a_key_that_is_only_the_prefix()
    {
        var exception = Assert.Throws<DomainValidationException>(
            () => PhotoKey.Create(PhotoKey.PrefixFor(Owner), Owner));

        Assert.Equal(PhotoKey.Errors.OutsideOwnerPrefix, exception.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_a_blank_key(string value)
    {
        var exception = Assert.Throws<DomainValidationException>(() => PhotoKey.Create(value, Owner));

        Assert.Equal(PhotoKey.Errors.Empty, exception.Code);
    }

    [Fact]
    public void Create_rejects_a_key_longer_than_the_maximum()
    {
        var value = PhotoKey.PrefixFor(Owner) + new string('a', PhotoKey.MaxLength);

        var exception = Assert.Throws<DomainValidationException>(() => PhotoKey.Create(value, Owner));

        Assert.Equal(PhotoKey.Errors.TooLong, exception.Code);
    }

    [Fact]
    public void Create_rejects_null()
    {
        Assert.Throws<ArgumentNullException>(() => PhotoKey.Create(null!, Owner));
    }

    [Fact]
    public void Equality_is_by_value()
    {
        var value = $"users/{Owner.Value}/a.jpg";

        Assert.Equal(PhotoKey.Create(value, Owner), PhotoKey.Create(value, Owner));
    }
}
