using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LibraryManagement.Data.Entities;

namespace LibraryManagement.Data.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.FullName).IsRequired();
        builder.Property(m => m.Email).IsRequired();
        builder.HasIndex(m => m.Email).IsUnique();
        builder.Property(m => m.OutstandingFine).HasDefaultValue(0).HasPrecision(18, 2);
    }
}
