using System.ComponentModel.DataAnnotations;

namespace Demo.MultiTenant.App.Entities
{
    public class Task : BaseEntity
    {
        private Task(Guid id, string tenantId, string name, Guid? userId) : base(id, tenantId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Task name cannot be empty.", nameof(name));
            }

            Name = name;
            UserId = userId;
        }

        [Required]
        [MaxLength(100)]
        public string Name { get; private set; }

        [MaxLength(1000)]
        public string? Description { get; private set; }

        public Guid? UserId { get; private set; }

        // Private parameterless constructor for EF Core
        private Task() : this(Guid.NewGuid(), string.Empty, string.Empty, null)
        {
        }

        public static Task Create(string tenantId, string name, string? description, Guid? userId)
        {
            var task = new Task(Guid.NewGuid(), tenantId, name, userId)
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
    }
}
