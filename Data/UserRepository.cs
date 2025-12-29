
using Entities;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class UserRepository : IUserRepository
    {
        readonly AppDbContext _context;
        public UserRepository(AppDbContext context) => _context = context;
        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<UserEntity?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }
        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);

        }

        public async Task AddAsync(UserEntity user)
        {

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }


    }
}
