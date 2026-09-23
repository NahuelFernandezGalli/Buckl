using Buckl.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Buckl.Infrastructure.Persistence.Configurations;

internal sealed class UserRecordConfiguration : IEntityTypeConfiguration<UserRecord>
{
    public void Configure(EntityTypeBuilder<UserRecord> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();

        // The snake case convention does not split on digits and would produce "auth0subject".
        builder.Property(user => user.Auth0Subject).HasColumnName("auth0_subject");
        builder.HasIndex(user => user.Auth0Subject)
            .IsUnique()
            .HasDatabaseName("users_auth0_subject_key");
    }
}
