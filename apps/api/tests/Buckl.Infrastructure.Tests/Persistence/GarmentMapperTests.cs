using Buckl.Domain.Garments;
using Buckl.Domain.Products;
using Buckl.Domain.Users;
using Buckl.Infrastructure.Persistence.Mapping;
using Buckl.Testing;

namespace Buckl.Infrastructure.Tests.Persistence;

public class GarmentMapperTests
{
    private static readonly UserId Owner = UserId.New();

    [Fact]
    public void A_garment_with_every_field_survives_the_round_trip()
    {
        var garment = TestGarments.Active(
            Owner,
            Category.Outerwear,
            Color.Navy,
            size: "L",
            notes: "Winter coat",
            productId: ProductId.New(),
            purchaseInfo: TestGarments.Purchase(120m, "EUR"),
            photoKey: TestGarments.PhotoFor(Owner));

        var restored = GarmentMapper.ToDomain(GarmentMapper.ToRecord(garment));

        GarmentAssertions.Same(garment, restored);
    }

    [Fact]
    public void An_archived_garment_survives_the_round_trip()
    {
        var garment = TestGarments.Archived(Owner);

        var restored = GarmentMapper.ToDomain(GarmentMapper.ToRecord(garment));

        GarmentAssertions.Same(garment, restored);
    }

    [Fact]
    public void ToRecord_writes_enumerations_as_lower_case_text()
    {
        var record = GarmentMapper.ToRecord(TestGarments.Active(Owner, Category.Outerwear, Color.Multicolor));

        Assert.Equal("outerwear", record.Category);
        Assert.Equal("multicolor", record.Color);
        Assert.Equal("manual", record.Source);
        Assert.Equal("active", record.Status);
    }

    [Fact]
    public void ToRecord_leaves_every_purchase_column_empty_without_purchase_information()
    {
        var record = GarmentMapper.ToRecord(TestGarments.Active(Owner));

        Assert.Null(record.PurchaseAmount);
        Assert.Null(record.PurchaseCurrency);
        Assert.Null(record.PurchaseDate);
    }

    [Fact]
    public void Apply_copies_the_changes_onto_an_existing_record_and_keeps_its_identity()
    {
        var garment = TestGarments.Active(Owner);
        var record = GarmentMapper.ToRecord(garment);
        garment.UpdateNotes("Needs ironing", TestClock.Now.AddHours(1));
        garment.Archive(TestClock.Now.AddHours(2));

        GarmentMapper.Apply(garment, record);

        Assert.Equal(garment.Id.Value, record.Id);
        Assert.Equal(TestClock.Now, record.CreatedAt);
        Assert.Equal("Needs ironing", record.Notes);
        Assert.Equal("archived", record.Status);
        Assert.Equal(TestClock.Now.AddHours(2), record.ArchivedAt);
        Assert.Equal(TestClock.Now.AddHours(2), record.UpdatedAt);
    }
}
