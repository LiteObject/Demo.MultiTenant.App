namespace Demo.MultiTenant.App.Entities
{
    public abstract class BaseEntity(Guid id, string tenantId)
    {
        public Guid Id { get; init; } = id;

        public string TenantId { get; init; } = tenantId;

        public DateTimeOffset CreatedOn { get; private set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset LastModifiedOn { get; private set; } = DateTimeOffset.UtcNow;

        public string? CreatedBy { get; private set; } = "System";

        public string? LastModifiedBy { get; private set; } = "System";

        public void UpdateModifiedOn()
        {
            LastModifiedOn = DateTimeOffset.UtcNow;
        }
    }
}
