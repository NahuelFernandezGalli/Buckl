using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Domain.Users;

namespace Buckl.Domain.Tests.Garments;

/// <summary>Valid garments for tests, so each test states only the data that matters to it.</summary>
internal static class GarmentMother
{
    public static readonly UserId DefaultOwner =
        new(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"));

    public static Classification BlueTop(Size? size = null) =>
        Classification.Create(Category.Top, Color.Blue, size ?? Size.Create("M"));

    public static PhotoKey PhotoFor(UserId owner, string name = "photo.jpg") =>
        PhotoKey.Create($"{PhotoKey.PrefixFor(owner)}garments/{name}", owner);

    public static Garment Active(UserId? ownerId = null, DateTimeOffset? now = null) =>
        Garment.Create(
            ownerId ?? DefaultOwner,
            BlueTop(),
            ImportSource.Manual,
            now ?? TestClock.Now);
}
