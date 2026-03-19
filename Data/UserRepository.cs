using Entities;
using Business.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class UserRepository : IUserRepository
{
    readonly AppDbContext _context;
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<UserEntity?> GetByEmailAsync(string email)
    {
        return await _context.users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }
    public async Task<UserEntity?> GetByIdAsync(int id)
    {
        return await _context.users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id );
    }
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.users
            .AnyAsync(u => u.Email == email);
    }
    public async Task<int> CreateAsync(UserEntity user)
    {
        await _context.users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user.Id;
    }
    public async Task UpdateAsync(UserEntity user)
    {
        _context.users.Update(user);
        await _context.SaveChangesAsync();
    }
    
    public async Task HardDeleteAsync(UserEntity user)
    {
        _context.users.Remove(user);
        await _context.SaveChangesAsync();
    }
}
