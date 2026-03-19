using DTO.User;
namespace Business.Interfaces
{
    public interface IUserService
    {
        Task<int> CreateAsync(CreateUserDTO UserDto);
        Task SoftDeleteAsync(int id, SoftUserDeleteDTO dto);
        Task<ReadUserDTO?> GetByIdAsync(int id);
        Task<ReadUserDTO?> GetByEmailAsync(string email);
        Task UpdatePasswordAsync(int id, UpdateUserPasswordDTO dto);
        Task UpdateRoleAsync(int id, UpdateUserRoleDTO dto);
        Task UpdateEmailAsync(int id,UpdateUserEmailDTO dto);
        Task AdminSoftDeleteAsync(int id);

        Task HardDeleteAsync(int id);
        Task RestoreUserAsync(int id);
        Task<PaginatedResponse<ReadUserDTO>> GetPagedAsync(PaginationFilterDTO filter);
    }
}
