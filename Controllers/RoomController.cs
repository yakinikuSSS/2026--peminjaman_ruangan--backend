using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PeminjamanRuangan.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    public class RoomController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        // GET ALL ROOMS
        [HttpGet]
        public async Task<IActionResult> GetAll(string? search, string? building, bool? isActive)
        {
            var query = _context.Rooms.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r =>
                    r.Name.Contains(search) ||
                    r.Code.Contains(search));
            }

            if (!string.IsNullOrEmpty(building))
                query = query.Where(r => r.Building.Contains(building));

            if (isActive.HasValue)
                query = query.Where(r => r.IsActive == isActive.Value);

            return Ok(await query.ToListAsync());
        }

        // GET ROOM BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
                return NotFound();

            return Ok(room);
        }

        // CREATE ROOM (ADMIN ONLY)
        [HttpPost]
        public async Task<IActionResult> Create(Room room, int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.Role != UserRole.Admin)
                return Forbid("Hanya admin yang boleh menambah ruangan.");

            if (string.IsNullOrWhiteSpace(room.Name))
                return BadRequest("Name wajib diisi.");

            if (string.IsNullOrWhiteSpace(room.Code))
                return BadRequest("Code wajib diisi.");

            var codeExists = await _context.Rooms
                .AnyAsync(r => r.Code == room.Code);

            if (codeExists)
                return BadRequest("Kode ruangan sudah digunakan.");

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return Ok(room);
        }

        // UPDATE ROOM (ADMIN ONLY)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Room updated, int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.Role != UserRole.Admin)
                return Forbid("Hanya admin yang boleh mengubah ruangan.");

            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(updated.Name))
                return BadRequest("Name wajib diisi.");

            if (string.IsNullOrWhiteSpace(updated.Code))
                return BadRequest("Code wajib diisi.");

            var codeExists = await _context.Rooms
                .AnyAsync(r => r.Code == updated.Code && r.Id != id);

            if (codeExists)
                return BadRequest("Kode ruangan sudah digunakan.");

            room.Name = updated.Name;
            room.Code = updated.Code;
            room.Building = updated.Building;
            room.Capacity = updated.Capacity;
            room.IsActive = updated.IsActive;

            await _context.SaveChangesAsync();

            return Ok(room);
        }

        // DELETE ROOM (DEACTIVATE - ADMIN ONLY)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.Role != UserRole.Admin)
                return Forbid("Hanya admin yang boleh menghapus ruangan.");

            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
                return NotFound();

            room.IsActive = false;

            await _context.SaveChangesAsync();

            return Ok("Room dinonaktifkan.");
        }
    }
}
