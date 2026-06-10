using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LibraryManagement.Data.Entities;

namespace LibraryManagement.Data.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Title).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Author).IsRequired();
        builder.Property(b => b.ISBN).IsRequired().HasMaxLength(13);
        builder.HasIndex(b => b.ISBN).IsUnique();
    }
}
