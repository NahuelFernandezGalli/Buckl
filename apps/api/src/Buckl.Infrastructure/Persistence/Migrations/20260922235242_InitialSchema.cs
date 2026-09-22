using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buckl.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reference_image_url = table.Column<string>(type: "text", nullable: true),
                    source_url = table.Column<string>(type: "text", nullable: true),
                    source = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.CheckConstraint("products_brand_not_blank", "brand is null or length(btrim(brand)) > 0");
                    table.CheckConstraint("products_image_url_https", "reference_image_url is null or reference_image_url like 'https://%'");
                    table.CheckConstraint("products_name_not_blank", "length(btrim(name)) > 0");
                    table.CheckConstraint("products_source_url_scheme", "source_url is null or source_url like 'http://%' or source_url like 'https://%'");
                    table.CheckConstraint("products_source_values", "source in ('manual', 'url', 'email')");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    auth0_subject = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "garments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    photo_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    category = table.Column<string>(type: "text", nullable: false),
                    color = table.Column<string>(type: "text", nullable: false),
                    size = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    purchase_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    purchase_currency = table.Column<string>(type: "character(3)", nullable: true),
                    purchase_date = table.Column<DateOnly>(type: "date", nullable: true),
                    source = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    archived_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_garments", x => x.id);
                    table.CheckConstraint("garments_archived_at_matches_status", "(status = 'archived' and archived_at is not null) or (status = 'active' and archived_at is null)");
                    table.CheckConstraint("garments_category_values", "category in ('top', 'bottom', 'dress', 'outerwear', 'footwear', 'accessory')");
                    table.CheckConstraint("garments_color_values", "color in ('black', 'white', 'grey', 'navy', 'blue', 'red', 'green', 'yellow', 'brown', 'beige', 'pink', 'purple', 'orange', 'multicolor')");
                    table.CheckConstraint("garments_photo_key_under_owner", "photo_key is null or photo_key like 'users/' || user_id::text || '/_%'");
                    table.CheckConstraint("garments_purchase_all_or_none", "(purchase_amount is null and purchase_currency is null and purchase_date is null) or (purchase_amount is not null and purchase_currency is not null and purchase_date is not null)");
                    table.CheckConstraint("garments_purchase_amount_not_negative", "purchase_amount is null or purchase_amount >= 0");
                    table.CheckConstraint("garments_purchase_currency_format", "purchase_currency is null or purchase_currency ~ '^[A-Z]{3}$'");
                    table.CheckConstraint("garments_size_not_blank", "size is null or length(btrim(size)) > 0");
                    table.CheckConstraint("garments_source_values", "source in ('manual', 'url', 'email')");
                    table.CheckConstraint("garments_status_values", "status in ('active', 'archived')");
                    table.CheckConstraint("garments_updated_not_before_created", "updated_at >= created_at");
                    table.ForeignKey(
                        name: "fk_garments_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_garments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "garments_owner_category_idx",
                table: "garments",
                columns: new[] { "user_id", "category" });

            migrationBuilder.CreateIndex(
                name: "garments_owner_status_idx",
                table: "garments",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "garments_product_idx",
                table: "garments",
                column: "product_id",
                filter: "product_id is not null");

            migrationBuilder.CreateIndex(
                name: "products_source_url_key",
                table: "products",
                column: "source_url",
                unique: true,
                filter: "source_url is not null");

            migrationBuilder.CreateIndex(
                name: "users_auth0_subject_key",
                table: "users",
                column: "auth0_subject",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "garments");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
