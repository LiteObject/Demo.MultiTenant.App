using Demo.MultiTenant.App.Entities;
using Demo.MultiTenant.App.Services;
using Microsoft.EntityFrameworkCore;

namespace Demo.MultiTenant.App.DAL
{
    public class TaskDbContext : DbContext
    {
        private readonly ITenantService _tenantService;

        public TaskDbContext(DbContextOptions<TaskDbContext> options, ITenantService tenantService) : base(options)
        {
            _tenantService = tenantService;
        }

        public DbSet<Entities.Task> Tasks { get; set; }
        public DbSet<Entities.User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities.Task>().ToTable("Tasks");
            modelBuilder.Entity<Entities.User>().ToTable("Users");

            // Apply global query filter for TenantId
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Check if the entity inherits from BaseEntity
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    // Use reflection to call the SetGlobalQueryFilter method
                    var method = typeof(TaskDbContext)
                        .GetMethod(nameof(SetGlobalQueryFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                        ?.MakeGenericMethod(entityType.ClrType);

                    // Invoke the method with the ModelBuilder and TenantService
                    method?.Invoke(null, new object[] { modelBuilder, _tenantService });
                }
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseAsyncSeeding(async (context, _, cancellationToken) =>
            {
                // Check if the database already has data
                var userExists = await context.Set<Entities.User>().AnyAsync(cancellationToken);
                var taskExists = await context.Set<Entities.Task>().AnyAsync(cancellationToken);

                // Seed Users if none exist
                if (!userExists)
                {
                    var user = User.Create("tenant-1", "john_doe", "john.doe@example.com", "John", "Doe");
                    context.Set<Entities.User>().Add(user);
                    await context.SaveChangesAsync(cancellationToken);

                    // Seed Tasks for the created user
                    if (!taskExists)
                    {
                        var task1 = Entities.Task.Create("tenant-1", "Complete project", "Finish the multi-tenant app project", user.Id);
                        var task2 = Entities.Task.Create("tenant-1", "Write documentation", "Document the API and codebase", user.Id);
                        context.Set<Entities.Task>().AddRange(task1, task2);
                        await context.SaveChangesAsync(cancellationToken);
                    }
                }
            }).UseSeeding((context, _) =>
            {
                // Check if the database already has data
                var userExists = context.Set<Entities.User>().Any();
                var taskExists = context.Set<Entities.Task>().Any();

                // Seed Users if none exist
                if (!userExists)
                {
                    var user = User.Create("tenant-1", "john_doe", "john.doe@example.com", "John", "Doe");
                    context.Set<Entities.User>().Add(user);
                    context.SaveChanges();

                    // Seed Tasks for the created user
                    if (!taskExists)
                    {
                        var task1 = Entities.Task.Create("tenant-1", "Complete project", "Finish the multi-tenant app project", user.Id);
                        var task2 = Entities.Task.Create("tenant-1", "Write documentation", "Document the API and codebase", user.Id);
                        context.Set<Entities.Task>().AddRange(task1, task2);
                        context.SaveChanges();
                    }
                }
            });

            // Configure the database connection here
            base.OnConfiguring(optionsBuilder);
        }

        private static void SetGlobalQueryFilter<TEntity>(ModelBuilder modelBuilder, ITenantService tenantService)
            where TEntity : BaseEntity
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == tenantService.GetCurrentTenantId());
        }

        /// <summary>
        /// Wrap all database operations that need to be executed within a transaction using the 
        /// ExecuteInTransactionAsync method when working with transactions in a retry-enabled context.
        /// This ensures that transient failures are handled gracefully.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task ExecuteInTransactionAsync(Func<System.Threading.Tasks.Task> action)
        {
            var executionStrategy = Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await Database.BeginTransactionAsync();
                try
                {
                    // Execute the action (e.g., database operations)
                    await action();

                    // Commit the transaction
                    await transaction.CommitAsync();
                }
                catch
                {
                    // Rollback the transaction in case of an error
                    await transaction.RollbackAsync();
                    throw; // Re-throw the exception
                }
            });
        }
    }
}
