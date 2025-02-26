using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Demo.MultiTenant.App.DAL.Configurations
{
    public class TaskConfiguration : IEntityTypeConfiguration<Entities.Task>
    {
        public void Configure(EntityTypeBuilder<Entities.Task> builder)
        {
            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(p => p.Description)
                   .HasMaxLength(1000);

            builder.Property(p => p.Description);

            // Optional: Add indexes for better query performance
            builder.HasIndex(p => p.Name);

            // Configure the relationship: A User can have many Tasks.
            // Using a shadow property "UserId" as the foreign key. 
            // If your Task entity defines a specific property for the foreign key, use it instead.
            builder.HasOne<Entities.User>()
                   .WithMany(u => u.Tasks)
                   .HasForeignKey("UserId")
                   .IsRequired();
        }
    }
}
