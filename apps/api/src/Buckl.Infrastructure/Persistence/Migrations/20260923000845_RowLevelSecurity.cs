using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Buckl.Infrastructure.Persistence.Migrations
{
    /// <summary>Row-Level Security on garments and the privileges of the application role
    /// (ADR-0007, docs/architecture/database-schema.md). Written by hand: EF Core has no model for
    /// policies or grants.</summary>
    public partial class RowLevelSecurity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                do $$
                begin
                    if not exists (select from pg_roles where rolname = 'buckl_app') then
                        raise exception 'Role buckl_app does not exist. Create it before migrating (apps/api/README.md).';
                    end if;
                end
                $$;
                """);

            migrationBuilder.Sql("alter table garments enable row level security;");
            migrationBuilder.Sql("alter table garments force row level security;");

            // nullif: after a transaction-local set_config the variable reads as '' on that
            // session, and ''::uuid would fail instead of matching no rows.
            migrationBuilder.Sql("""
                create policy garments_owner on garments
                    using      (user_id = nullif(current_setting('app.user_id', true), '')::uuid)
                    with check (user_id = nullif(current_setting('app.user_id', true), '')::uuid);
                """);

            migrationBuilder.Sql("grant usage on schema public to buckl_app;");
            migrationBuilder.Sql("grant select, insert on users to buckl_app;");
            migrationBuilder.Sql("grant select, insert on products to buckl_app;");
            migrationBuilder.Sql("grant select, insert, update on garments to buckl_app;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("revoke select, insert, update on garments from buckl_app;");
            migrationBuilder.Sql("revoke select, insert on products from buckl_app;");
            migrationBuilder.Sql("revoke select, insert on users from buckl_app;");
            migrationBuilder.Sql("revoke usage on schema public from buckl_app;");
            migrationBuilder.Sql("drop policy garments_owner on garments;");
            migrationBuilder.Sql("alter table garments no force row level security;");
            migrationBuilder.Sql("alter table garments disable row level security;");
        }
    }
}
