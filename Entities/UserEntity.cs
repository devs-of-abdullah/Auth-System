

namespace Entities
{
    public class UserEntity
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

    }
}
