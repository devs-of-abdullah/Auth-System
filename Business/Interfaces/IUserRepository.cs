using Entities;
using DTO.User;

namespace Business.Interfaces
{
    public interface IUserRepository
    {
        Task<UserEntity?> GetByEmailAsync(string email);
        Task<UserEntity?> GetByIdAsync(int id);
        Task<bool> ExistsByEmailAsync(string email);
        Task<int> CreateAsync(UserEntity user);
        Task UpdateAsync(UserEntity user);

        Task HardDeleteAsync(UserEntity user);
        Task<(IEnumerable<UserEntity> Users, int TotalCount)> GetPagedAsync(PaginationFilterDTO filter);
    }
}
