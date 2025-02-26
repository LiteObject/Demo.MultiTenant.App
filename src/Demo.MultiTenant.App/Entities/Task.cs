using System.ComponentModel.DataAnnotations;

namespace Demo.MultiTenant.App.Entities
{
    public class Task : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; private set; }

        [MaxLength(1000)]
        public string? Description { get; private set; }

        public Guid? UserId { get; private set; }

        // Private parameterless constructor for EF Core
        private Task() : this(string.Empty, string.Empty, null)
        {
        }

        private Task(string tenantId, string name, Guid? userId)
            : base(Guid.NewGuid(), tenantId)
        {           

            Name = name;
            UserId = userId;
        }

        public static Task Create(string tenantId, string name, string? description, Guid? userId)
        {
            ValidateInputs(tenantId, name);

            var task = new Task(tenantId, name, userId)
            {
                Description = description
            };

            return task;
        }

        // Optionally, add methods to update fields in a controlled manner
        public void UpdateName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("Task name cannot be empty.", nameof(newName));
            }

            Name = newName;
            UpdateModifiedOn();
        }

        public void UpdateDescription(string? newDescription)
        {
            Description = newDescription;
            UpdateModifiedOn();
        }

        public void ReassignUser(Guid newUserId)
        {
            UserId = newUserId;
            UpdateModifiedOn();
        }

        private static void ValidateInputs(string tenantId, string name)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Task name cannot be empty.", nameof(name));
            }
        }
    }
}
