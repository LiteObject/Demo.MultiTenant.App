using Demo.MultiTenant.App.Components;
using Demo.MultiTenant.App.DAL;
using Demo.MultiTenant.App.Services;
using Microsoft.EntityFrameworkCore;

namespace Demo.MultiTenant.App;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add explicit configuration (optional if you need specific options)
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                               "Could not find a connection string named 'DefaultConnection'.");
        }

        // Required for IHttpContextAccessor
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantService, TenantService>();

        // Register the ProductContext with the dependency injection container
        builder.Services.AddDbContext<TaskDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
            }));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        //await using var serviceScope = app.Services.CreateAsyncScope();
        //await using var dbContext = serviceScope.ServiceProvider.GetRequiredService<TaskDbContext>();
        //await dbContext.Database.EnsureCreatedAsync();

        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<Components.App>() // Ensure the correct namespace and class name
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
