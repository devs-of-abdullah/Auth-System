
namespace Business
{
    public interface IUserService
    {
        Task RegisterAsync(string email, string password);
        Task<string> LoginAsync(string email, string password);
    }
}
