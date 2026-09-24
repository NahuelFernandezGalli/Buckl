using Buckl.Domain.Common;
using Buckl.Domain.Products;
using Buckl.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Buckl.Infrastructure.Persistence.Configurations;

internal sealed class ProductRecordConfiguration : IEntityTypeConfiguration<ProductRecord>
{
    public void Configure(EntityTypeBuilder<ProductRecord> builder)
    {
        builder.ToTable("products", table =>
        {
            table.HasCheckConstraint("products_name_not_blank", "length(btrim(name)) > 0");
            table.HasCheckConstraint(
                "products_brand_not_blank",
                "brand is null or length(btrim(brand)) > 0");
            table.HasCheckConstraint(
                "products_image_url_https",
                "reference_image_url is null or reference_image_url like 'https://%'");
            table.HasCheckConstraint(
                "products_source_url_scheme",
                "source_url is null or source_url like 'http://%' or source_url like 'https://%'");
            table.HasCheckConstraint(
                "products_source_values",
                $"source in ({EnumText.SqlList<ImportSource>()})");
        });

        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).ValueGeneratedNever();
        builder.Property(product => product.Name).HasMaxLength(Product.MaxNameLength);
        builder.Property(product => product.Brand).HasMaxLength(Product.MaxBrandLength);
        builder.HasIndex(product => product.SourceUrl)
            .IsUnique()
            .HasFilter("source_url is not null")
            .HasDatabaseName("products_source_url_key");
    }
}
