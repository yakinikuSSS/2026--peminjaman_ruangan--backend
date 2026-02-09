using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace PeminjamanRuangan.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        // HASH PASSWORD
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // REGISTER CUSTOMER
        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Name))
                return BadRequest("Name wajib diisi.");

            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest("Email wajib diisi.");

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                return BadRequest("Password wajib diisi.");

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == user.Email);

            if (emailExists)
                return BadRequest("Email sudah digunakan.");

            user.PasswordHash = HashPassword(user.PasswordHash);
            user.Role = UserRole.Customer;
            user.CreatedDate = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registrasi berhasil",
                user.Id,
                user.Name,
                user.Email,
                user.Role
            });
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == email &&
                    u.DeletedAt == null);

            if (user == null)
                return Unauthorized("Email tidak ditemukan.");

            if (!user.IsActive)
                return Unauthorized("Akun tidak aktif.");

            var hashedInput = HashPassword(password);

            if (user.PasswordHash != hashedInput)
                return Unauthorized("Password salah.");

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role
            });
        }
    }
}
