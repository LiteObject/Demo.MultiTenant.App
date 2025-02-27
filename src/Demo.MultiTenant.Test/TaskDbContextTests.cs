using Demo.MultiTenant.App.DAL;
using Demo.MultiTenant.App.Services;
using Microsoft.EntityFrameworkCore;

namespace Demo.MultiTenant.Test
{
    public class TaskDbContextTests
    {
        [Fact]
        public async Task GlobalQueryFilter_ReturnsOnlyTasksForCurrentTenant()
        {
            // Arrange: configure in-memory database
            DbContextOptions<TaskDbContext> options = new DbContextOptionsBuilder<TaskDbContext>()
                .UseInMemoryDatabase(databaseName: "GlobalFilterTestDb1")
                .Options;

            // Use tenant-1 for this test
            FakeTenantService tenantServiceTenant1 = new("tenant-1");

            // Seed the database with tasks from two different tenants
            using (TaskDbContext context = new (options, tenantServiceTenant1))
            {
                App.Entities.Task taskTenant1 = App.Entities.Task.Create("tenant-1", "Task for tenant1", "Description", Guid.NewGuid());
                App.Entities.Task taskTenant2 = App.Entities.Task.Create("tenant-2", "Task for tenant2", "Description", Guid.NewGuid());

                context.Tasks.AddRange(taskTenant1, taskTenant2);
                await context.SaveChangesAsync();
            }

            // Act: Query with the tenant service set to tenant-1.
            using (TaskDbContext context = new(options, tenantServiceTenant1))
            {
                List<App.Entities.Task> tasks = await context.Tasks.ToListAsync();

                // Assert: Only the task for tenant-1 should be returned.
                Assert.Single(tasks);
                Assert.All(tasks, task => Assert.Equal("tenant-1", task.TenantId));
            }
        }
    }

    public class FakeTenantService(string tenantId) : ITenantService
    {
        private readonly string _tenantId = tenantId;

        public string GetCurrentTenantId() => _tenantId;
    }

}
