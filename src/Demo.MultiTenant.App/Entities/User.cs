
namespace Demo.MultiTenant.App.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; private set; }
        public string? FirstName { get; private set; }
        public string? LastName { get; private set; }
        public string Email { get; private set; }

        public ICollection<Task> Tasks { get; private set; } = new List<Task>();

        // Private constructor for EF Core
        private User() : this(Guid.NewGuid(), string.Empty, string.Empty, string.Empty)
        {
        }

        private User(Guid id, string tenantId, string username, string email) : base(id, tenantId)
        {
            Username = username;
            Email = email;
        }

        private static void ValidateInputs(string username, string email)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be empty", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be empty", nameof(email));
            }
        }

        public static User Create(string tenantId, string username, string email, string? firstName = null, string? lastName = null)
        {
            ValidateInputs(username, email);

            return new User(Guid.NewGuid(), tenantId, username, email)
            {
                FirstName = firstName,
                LastName = lastName
            };
        }

        public void Update(string username, string email, string? firstName = null, string? lastName = null)
        {
            ValidateInputs(username, email);

            Username = username;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            UpdateModifiedOn();
        }
    }
}
