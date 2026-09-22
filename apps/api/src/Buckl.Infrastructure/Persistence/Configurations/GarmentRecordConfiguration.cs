using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Buckl.Infrastructure.Persistence.Configurations;

/// <summary>Every constraint mirrors a domain rule, so a bug in application code cannot store a
/// state the domain would have rejected (docs/architecture/database-schema.md).</summary>
internal sealed class GarmentRecordConfiguration : IEntityTypeConfiguration<GarmentRecord>
{
    public void Configure(EntityTypeBuilder<GarmentRecord> builder)
    {
        builder.ToTable("garments", table =>
        {
            table.HasCheckConstraint(
                "garments_category_values",
                $"category in ({EnumText.SqlList<Category>()})");
            table.HasCheckConstraint(
                "garments_color_values",
                $"color in ({EnumText.SqlList<Color>()})");
            table.HasCheckConstraint(
                "garments_source_values",
                $"source in ({EnumText.SqlList<ImportSource>()})");
            table.HasCheckConstraint(
                "garments_status_values",
                $"status in ({EnumText.SqlList<GarmentStatus>()})");
            table.HasCheckConstraint(
                "garments_photo_key_under_owner",
                "photo_key is null or photo_key like 'users/' || user_id::text || '/_%'");
            table.HasCheckConstraint(
                "garments_size_not_blank",
                "size is null or length(btrim(size)) > 0");
            table.HasCheckConstraint(
                "garments_purchase_amount_not_negative",
                "purchase_amount is null or purchase_amount >= 0");
            table.HasCheckConstraint(
                "garments_purchase_currency_format",
                "purchase_currency is null or purchase_currency ~ '^[A-Z]{3}$'");
            table.HasCheckConstraint(
                "garments_purchase_all_or_none",
                "(purchase_amount is null and purchase_currency is null and purchase_date is null) or "
                + "(purchase_amount is not null and purchase_currency is not null and purchase_date is not null)");
            table.HasCheckConstraint(
                "garments_archived_at_matches_status",
                "(status = 'archived' and archived_at is not null) or (status = 'active' and archived_at is null)");
            table.HasCheckConstraint(
                "garments_updated_not_before_created",
                "updated_at >= created_at");
        });

        builder.HasKey(garment => garment.Id);
        builder.Property(garment => garment.Id).ValueGeneratedNever();
        builder.Property(garment => garment.PhotoKey).HasMaxLength(PhotoKey.MaxLength);
        builder.Property(garment => garment.Size).HasMaxLength(Size.MaxLength);
        builder.Property(garment => garment.PurchaseAmount).HasPrecision(12, 2);
        builder.Property(garment => garment.PurchaseCurrency).HasColumnType("character(3)");
        builder.Property(garment => garment.Notes).HasMaxLength(Garment.MaxNotesLength);

        builder.HasOne<UserRecord>()
            .WithMany()
            .HasForeignKey(garment => garment.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(garment => garment.Product)
            .WithMany()
            .HasForeignKey(garment => garment.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(garment => new { garment.UserId, garment.Status })
            .HasDatabaseName("garments_owner_status_idx");
        builder.HasIndex(garment => new { garment.UserId, garment.Category })
            .HasDatabaseName("garments_owner_category_idx");
        builder.HasIndex(garment => garment.ProductId)
            .HasFilter("product_id is not null")
            .HasDatabaseName("garments_product_idx");
    }
}
