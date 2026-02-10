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
        public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> GetAll(string? search, bool? isActive)
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

            var customers = await query.ToListAsync();

            var result = customers.Select(u => new CustomerResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                Address = u.Address,
                IsActive = u.IsActive,
                CreatedDate = u.CreatedDate
            });

            return Ok(result);
        }

        // GET /customers/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerResponseDto>> GetById(int id)
        {
            var u = await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.Role == UserRole.Customer &&
                    x.DeletedAt == null);

            if (u == null)
                return NotFound();

            var result = new CustomerResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                Address = u.Address,
                IsActive = u.IsActive,
                CreatedDate = u.CreatedDate
            };

            return Ok(result);
        }

        // POST /customers
        [HttpPost]
        public async Task<ActionResult<CustomerResponseDto>> Create(CreateCustomerDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Name wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email wajib diisi.");

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email);

            if (emailExists)
                return BadRequest("Email sudah digunakan.");

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                Role = UserRole.Customer,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = new CustomerResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate
            };

            return Ok(result);
        }

        // PUT /customers/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<CustomerResponseDto>> Update(int id, UpdateCustomerDto request)
        {
            var customer = await _context.Users.FindAsync(id);

            if (customer == null ||
                customer.Role != UserRole.Customer ||
                customer.DeletedAt != null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Name wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email wajib diisi.");

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email && u.Id != id);

            if (emailExists)
                return BadRequest("Email sudah digunakan.");

            customer.Name = request.Name;
            customer.Email = request.Email;
            customer.Phone = request.Phone;
            customer.Address = request.Address;
            customer.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            var result = new CustomerResponseDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                IsActive = customer.IsActive,
                CreatedDate = customer.CreatedDate
            };

            return Ok(result);
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
