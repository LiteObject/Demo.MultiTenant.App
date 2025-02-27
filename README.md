# Demo Multi-Tenant App

Multi-tenancy is a software architecture where a single instance of the application serves multiple tenants (customers or organizations), with data isolation between them. EF Core supports multi-tenancy through various strategies, and the article highlights the following approaches:

### Database per Tenant
- Each tenant has its own dedicated database.
- EF Core can dynamically switch the database connection based on the tenant context.
- This approach provides strong data isolation but can be resource-intensive.

### Schema per Tenant
- All tenants share the same database, but each tenant has its own schema within that database.
- EF Core can dynamically switch the schema based on the tenant context.
- This approach balances isolation and resource usage but requires careful schema management.

### Row-Level Tenancy (Shared Database)
- All tenants share the same database and schema, but each row in a table is associated with a tenant identifier (e.g., `TenantId`).
- EF Core can filter queries based on the `TenantId` to ensure data isolation.
- This approach is resource-efficient but requires careful query design to avoid data leaks.

## Key Considerations:

- **Tenant Identification**: The application must identify the tenant for each request, typically using a tenant identifier in the request (e.g., URL, header, or user claim).

- **Connection Management**: For database-per-tenant or schema-per-tenant, EF Core must dynamically manage database connections or schema contexts.

- **Query Filtering**: For row-level tenancy, EF Core's global query filters can automatically apply tenant-specific filters to queries.

- **Migrations**: Managing database migrations can be more complex in multi-tenant scenarios, especially with database-per-tenant or schema-per-tenant strategies.

## Implementation Tips:
- Use dependency injection to manage tenant-specific contexts.
- Leverage EF Core's DbContext factory pattern for dynamic context creation.
- Use global query filters for row-level tenancy to ensure tenant isolation.

## Example: Dependency injection to manage tenant-specific contexts

### Step 1: Define a Tenant Service
First, create a service to resolve the tenant information (e.g., from the request).

```csharp
public interface ITenantService
{
    string GetCurrentTenantId();
}

public class TenantService(IHttpContextAccessor httpContextAccessor) : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public string GetCurrentTenantId()
        {
            // Retrieve the tenant ID from the current HTTP context (e.g., from a claim or header)
            return _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId")?.Value ?? "default-tenant";
        }
    }
```

### Step 2: Create a Tenant-Specific DbContext
Create a custom `DbContext` that can be configured dynamically based on the tenant.

```csharp
public class TenantDbContext : DbContext
{
    private readonly string _tenantId;

    public TenantDbContext(DbContextOptions<TenantDbContext> options, string tenantId)
        : base(options)
    {
        _tenantId = tenantId;
    }

    public DbSet<MyEntity> MyEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply tenant-specific query filters
        modelBuilder.Entity<MyEntity>().HasQueryFilter(e => e.TenantId == _tenantId);
    }
}
```

### Step 3: Register the DbContext Factory in DI
Use a factory pattern to create tenant-specific `DbContext` instances.

```csharp
public class TenantDbContextFactory
{
    private readonly IDbContextFactory<TenantDbContext> _pooledFactory;
    private readonly ITenantService _tenantService;

    public TenantDbContextFactory(IDbContextFactory<TenantDbContext> pooledFactory, ITenantService tenantService)
    {
        _pooledFactory = pooledFactory;
        _tenantService = tenantService;
    }

    public TenantDbContext CreateDbContext()
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseYourDatabaseProvider("YourConnectionString") // Replace with your database provider and connection string
            .Options;

        return new TenantDbContext(options, tenantId);
    }
}
```

### Step 4: Register Services
Register the tenant service, `DbContext` factory, and other dependencies in the DI container.

```csharp
services.AddHttpContextAccessor();
services.AddScoped<ITenantService, TenantService>();

// Register the DbContext factory
services.AddDbContextFactory<TenantDbContext>((provider, options) =>
{
    var tenantService = provider.GetRequiredService<ITenantService>();
    var tenantId = tenantService.GetCurrentTenantId();
    options.UseYourDatabaseProvider("YourConnectionString"); // Replace with your database provider and connection string
});

services.AddScoped<TenantDbContextFactory>();
```

EF Core provides flexible support for multi-tenancy through database-per-tenant, schema-per-tenant, and row-level tenancy strategies. The choice of strategy depends on the application's requirements for data isolation, scalability, and resource usage.

---
## Commands to create and apply migration

### Add migration files

```bash
dotnet ef migrations add InitialCreate -p src/Demo.MultiTenant.App -s src/Demo.MultiTenant.App 
```

### Apply migration

```bash
dotnet ef database update -p src/Demo.MultiTenant.App -s src/Demo.MultiTenant.App
```


---
## Links:
- [Global Query Filters](https://learn.microsoft.com/en-us/ef/core/querying/filters)
- [Multi-tenancy](https://learn.microsoft.com/en-us/ef/core/miscellaneous/multitenancy)