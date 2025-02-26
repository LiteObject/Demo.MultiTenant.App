using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Demo.MultiTenant.App.DAL.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<Entities.User>
    {
        public void Configure(EntityTypeBuilder<Entities.User> builder)
        {
            builder.Property(p => p.Username)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(p => p.FirstName)
                   .HasMaxLength(50);

            builder.Property(p => p.LastName)
                   .HasMaxLength(50);

            builder.Property(p => p.Email)
                   .IsRequired()
                   .HasMaxLength(100);

            // Optional: Add indexes for better query performance
            builder.HasIndex(p => p.Username);
        }
    }
}
