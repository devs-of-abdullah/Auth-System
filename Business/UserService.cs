using Data;
using Entities;
namespace Business
{
    public class UserService : IUserService
    {
        readonly IUserRepository _repo;
        readonly TokenService _tokenService;
        public UserService(IUserRepository repo, TokenService token)
        {
            _repo = repo;
            _tokenService = token;
        }

        public async Task RegisterAsync(string email, string password)
        {
            if (await _repo.ExistsByEmailAsync(email))
                throw new InvalidOperationException("User already exists");

            var user = new UserEntity
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            await _repo.AddAsync(user);


        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await _repo.GetByEmailAsync(email)
                ?? throw new UnauthorizedAccessException("Invalid credintials");

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new InvalidOperationException("Invalid credintials");

            return _tokenService.CreateToken(user);

        }


    }

}
