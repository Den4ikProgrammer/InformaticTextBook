using Microsoft.EntityFrameworkCore;
using ServiceLayer.Data;
using ServiceLayer.Models;
using System.Security.Cryptography;
using System.Text;

namespace ServiceLayer.Services
{
    public class UserService
    {
        private readonly InformaticTextBookContext _context;

        public UserService(InformaticTextBookContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByLoginAndPasswordAsync(string login, string password)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserLogin == login);

            if (user == null) return null;

            string hash = HashPassword(password);
            bool isValid = hash == user.UserPassword;

            return isValid ? user : null;
        }

        // Хэширование (SHA-256 без соли)
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<List<User>> GetAllStudentsAsync()
        {
            return await _context.Users.Where(u => u.RoleId == 2).Include(u => u.Role).ToListAsync();
        }
    }
}
