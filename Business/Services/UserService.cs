using Business.Interfaces;
using Business.Constants;
using DTO.User;
using Entities;
namespace Business.Services
{
    public class UserService : IUserService
    {
        readonly IUserRepository _repo;
        readonly IEmailService _emailService;
        public UserService(IUserRepository repo, IEmailService emailService)
        {
            _repo = repo;
            _emailService = emailService;
        }
        public async Task<int> CreateAsync(CreateUserDTO dto)
        {
            if (await _repo.ExistsByEmailAsync(dto.Email))
                throw new InvalidOperationException($"'{dto.Email}' email already exists");


            var verificationToken = Random.Shared.Next(1000, 10000).ToString();

            var user = new UserEntity
            {
                Email = dto.Email,
                Role = dto.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsEmailVerified = false,
                EmailVerificationToken = BCrypt.Net.BCrypt.HashPassword(verificationToken),
                EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
                EmailVerificationTokenSentAt = DateTime.UtcNow
            };

            var userId = await _repo.CreateAsync(user);

            var emailBody = EmailTemplates.GetVerificationEmailBody(verificationToken);
            await _emailService.SendEmailAsync(user.Email, EmailTemplates.VerificationEmailSubject, emailBody);

            return userId;


        }
        public async Task<ReadUserDTO?> GetByIdAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null) return null;


            return new ReadUserDTO
            {
                Id = user.Id,
                Role = user.Role,
                Email = user.Email,
            };
        }
        public async Task<ReadUserDTO?> GetByEmailAsync(string email)
        {
            var user = await _repo.GetByEmailAsync(email);
            if (user == null) return null;

            return new ReadUserDTO
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
            };
        }
        public async Task SoftDeleteAsync(int Id, SoftUserDeleteDTO dto)
        {
            var user = await _repo.GetByIdAsync(Id);

            if (user == null)
                throw new KeyNotFoundException("User not found");
           
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect");
            
            if (user.IsDeleted)
                throw new InvalidOperationException("User already deleted");
           
            user.IsDeleted = true;

            await _repo.UpdateAsync(user);
        }
        public async Task UpdatePasswordAsync(int userId, UpdateUserPasswordDTO dto)
        {
            var user = await _repo.GetByIdAsync(userId);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect");

            if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
                throw new InvalidOperationException("New password cannot be same as old password");

            if (dto.NewPassword.Length < 6)
                throw new InvalidOperationException("Password must be at least 6 characters.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 12);

            user.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(user);
        }
        public async Task UpdateRoleAsync(int id, UpdateUserRoleDTO dto)
        {         
           
            var user = await _repo.GetByIdAsync(id);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.Role = dto.NewRole;
            await _repo.UpdateAsync(user);

        }
        public async Task UpdateEmailAsync(int userId, UpdateUserEmailDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NewEmail))
                throw new ArgumentException("Email cannot be empty");

            var normalizedEmail = dto.NewEmail.Trim().ToLower();

            var user = await _repo.GetByIdAsync(userId);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (user.IsDeleted)
                throw new InvalidOperationException("User account is deleted");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Password is incorrect");

            if (user.Email == normalizedEmail)
                throw new InvalidOperationException("New email cannot be same as current email");

            var existing = await _repo.GetByEmailAsync(normalizedEmail);

            if (existing != null && existing.Id != userId)
                throw new InvalidOperationException("Email already in use");

            var verificationToken = Random.Shared.Next(1000, 10000).ToString();

            user.Email = normalizedEmail;
            user.IsEmailVerified = false;
            user.EmailVerificationToken = BCrypt.Net.BCrypt.HashPassword(verificationToken);
            user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);
            user.EmailVerificationTokenSentAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(user);

            var emailBody = EmailTemplates.GetVerificationEmailBody(verificationToken);
            await _emailService.SendEmailAsync(user.Email, EmailTemplates.VerificationEmailSubject, emailBody);
        }
        public async Task AdminSoftDeleteAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("User not found");

            if (user.IsDeleted)
                throw new InvalidOperationException("User already deleted");

            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(user);
        }

        public async Task HardDeleteAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("User not found");

            await _repo.HardDeleteAsync(user);
        }

        public async Task RestoreUserAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("User not found");

            if (!user.IsDeleted)
                throw new InvalidOperationException("User is not currently soft-deleted");

            user.IsDeleted = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(user);
        }

        public async Task<PaginatedResponse<ReadUserDTO>> GetPagedAsync(PaginationFilterDTO filter)
        {
            var (users, totalCount) = await _repo.GetPagedAsync(filter);

            var userDtos = users.Select(u => new ReadUserDTO
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            });

            return new PaginatedResponse<ReadUserDTO>(userDtos, totalCount, filter.PageNumber, filter.PageSize);
        }
    }

}