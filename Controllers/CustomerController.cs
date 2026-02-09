using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PeminjamanRuangan.Controllers
{
    [ApiController]
    [Route("customers")]
    public class CustomerController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        // GET /customers
        [HttpGet]
        public async Task<IActionResult> GetAll(string? search, bool? isActive)
        {
            var query = _context.Users
                .Where(u => u.Role == UserRole.Customer && u.DeletedAt == null)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u =>
                    u.Name.Contains(search) ||
                    u.Email.Contains(search));
            }

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            return Ok(await query.ToListAsync());
        }

        // GET /customers/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id == id &&
                    u.Role == UserRole.Customer &&
                    u.DeletedAt == null);

            if (customer == null)
                return NotFound();

            return Ok(customer);
        }

        // POST /customers
        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Name))
                return BadRequest("Name wajib diisi.");

            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest("Email wajib diisi.");

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == user.Email);

            if (emailExists)
                return BadRequest("Email sudah digunakan.");

            user.Role = UserRole.Customer;
            user.CreatedDate = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        // PUT /customers/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, User updated)
        {
            var customer = await _context.Users.FindAsync(id);

            if (customer == null ||
                customer.Role != UserRole.Customer ||
                customer.DeletedAt != null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(updated.Name))
                return BadRequest("Name wajib diisi.");

            if (string.IsNullOrWhiteSpace(updated.Email))
                return BadRequest("Email wajib diisi.");

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == updated.Email && u.Id != id);

            if (emailExists)
                return BadRequest("Email sudah digunakan.");

            customer.Name = updated.Name;
            customer.Email = updated.Email;
            customer.Phone = updated.Phone;
            customer.Address = updated.Address;
            customer.IsActive = updated.IsActive;

            await _context.SaveChangesAsync();

            return Ok(customer);
        }

        // DELETE /customers/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Users.FindAsync(id);

            if (customer == null ||
                customer.Role != UserRole.Customer ||
                customer.DeletedAt != null)
                return NotFound();

            customer.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }
    }
}
