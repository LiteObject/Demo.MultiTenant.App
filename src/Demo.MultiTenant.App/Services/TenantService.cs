namespace Demo.MultiTenant.App.Services
{
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentTenantId()
        {
            // Retrieve the tenant ID from the current HTTP context (e.g., from a claim or header)
            return _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId")?.Value ?? "default-tenant";
        }
    }
}
