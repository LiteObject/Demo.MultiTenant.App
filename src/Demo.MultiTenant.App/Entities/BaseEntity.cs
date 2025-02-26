using System.ComponentModel.DataAnnotations;

namespace Demo.MultiTenant.App.Entities
{
    public abstract class BaseEntity(Guid id, string tenantId)
    {
        public Guid Id { get; private init; } = id;

        public string TenantId { get; private init; } = tenantId;

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
