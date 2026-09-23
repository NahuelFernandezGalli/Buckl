using Buckl.Domain.Garments;

namespace Buckl.Infrastructure.Tests.Persistence;

internal static class GarmentAssertions
{
    public static void Same(Garment expected, Garment actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.OwnerId, actual.OwnerId);
        Assert.Equal(expected.Classification, actual.Classification);
        Assert.Equal(expected.Source, actual.Source);
        Assert.Equal(expected.Status, actual.Status);
        Assert.Equal(expected.PhotoKey, actual.PhotoKey);
        Assert.Equal(expected.PurchaseInfo, actual.PurchaseInfo);
        Assert.Equal(expected.ProductId, actual.ProductId);
        Assert.Equal(expected.Notes, actual.Notes);
        Assert.Equal(expected.CreatedAt, actual.CreatedAt);
        Assert.Equal(expected.UpdatedAt, actual.UpdatedAt);
        Assert.Equal(expected.ArchivedAt, actual.ArchivedAt);
    }
}
