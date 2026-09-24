using Buckl.Domain.Common;
using Buckl.Domain.Garments;
using Buckl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Buckl.Infrastructure.Tests.Persistence;

/// <summary>Keeps the EF model in step with docs/architecture/database-schema.md: the DDL the model
/// produces must contain every table, column type, constraint and index the document
/// states.</summary>
public class BucklDbContextModelTests
{
    private const string OfflineConnectionString = "Host=localhost;Database=buckl_model";

    private static readonly string Script = CreateScript();

    public static TheoryData<string> CheckConstraints =>
    [
        "products_name_not_blank",
        "products_brand_not_blank",
        "products_image_url_https",
        "products_source_url_scheme",
        "products_source_values",
        "garments_category_values",
        "garments_color_values",
        "garments_source_values",
        "garments_status_values",
        "garments_photo_key_under_owner",
        "garments_size_not_blank",
        "garments_purchase_amount_not_negative",
        "garments_purchase_currency_format",
        "garments_purchase_all_or_none",
        "garments_archived_at_matches_status",
        "garments_updated_not_before_created",
    ];

    [Theory]
    [InlineData("CREATE TABLE users (")]
    [InlineData("CREATE TABLE products (")]
    [InlineData("CREATE TABLE garments (")]
    public void The_model_creates_the_tables_of_the_schema(string fragment) =>
        Assert.Contains(fragment, Script, StringComparison.Ordinal);

    [Theory]
    [InlineData("auth0_subject text NOT NULL")]
    [InlineData("name character varying(200) NOT NULL")]
    [InlineData("brand character varying(100)")]
    [InlineData("reference_image_url text")]
    [InlineData("user_id uuid NOT NULL")]
    [InlineData("photo_key character varying(512)")]
    [InlineData("size character varying(20)")]
    [InlineData("purchase_amount numeric(12,2)")]
    [InlineData("purchase_currency character(3)")]
    [InlineData("purchase_date date")]
    [InlineData("notes character varying(500)")]
    [InlineData("created_at timestamp with time zone NOT NULL")]
    [InlineData("archived_at timestamp with time zone")]
    public void Columns_have_the_types_of_the_schema(string column) =>
        Assert.Contains(column, Script, StringComparison.Ordinal);

    [Theory]
    [MemberData(nameof(CheckConstraints))]
    public void Every_check_constraint_of_the_schema_is_declared(string name) =>
        Assert.Contains($"CONSTRAINT {name} CHECK", Script, StringComparison.Ordinal);

    [Fact]
    public void Enumeration_constraints_list_every_enum_member()
    {
        Assert.Contains($"category in ({EnumText.SqlList<Category>()})", Script, StringComparison.Ordinal);
        Assert.Contains($"color in ({EnumText.SqlList<Color>()})", Script, StringComparison.Ordinal);
        Assert.Contains($"status in ({EnumText.SqlList<GarmentStatus>()})", Script, StringComparison.Ordinal);
        Assert.Contains($"source in ({EnumText.SqlList<ImportSource>()})", Script, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("CREATE UNIQUE INDEX users_auth0_subject_key ON users (auth0_subject);")]
    [InlineData("CREATE UNIQUE INDEX products_source_url_key ON products (source_url) WHERE source_url is not null;")]
    [InlineData("CREATE INDEX garments_owner_status_idx ON garments (user_id, status);")]
    [InlineData("CREATE INDEX garments_owner_category_idx ON garments (user_id, category);")]
    [InlineData("CREATE INDEX garments_product_idx ON garments (product_id) WHERE product_id is not null;")]
    public void Indexes_match_the_schema(string index) =>
        Assert.Contains(index, Script, StringComparison.Ordinal);

    [Theory]
    [InlineData("REFERENCES users (id) ON DELETE CASCADE")]
    [InlineData("REFERENCES products (id) ON DELETE RESTRICT")]
    public void Foreign_keys_match_the_schema(string reference) =>
        Assert.Contains(reference, Script, StringComparison.Ordinal);

    private static string CreateScript()
    {
        using var context = new BucklDbContext(BucklDbContextOptions.Build(OfflineConnectionString));

        return context.Database.GenerateCreateScript();
    }
}
