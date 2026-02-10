using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeminjamanRuangan.DTOs.Room;

namespace PeminjamanRuangan.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    public class RoomController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        // GET ALL ROOMS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomResponseDto>>> GetAll(
            string? search,
            string? building,
            bool? isActive)
        {
            var query = _context.Rooms.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(r =>
                    r.Name.Contains(search) ||
                    r.Code.Contains(search));

            if (!string.IsNullOrEmpty(building))
                query = query.Where(r => r.Building.Contains(building));

            if (isActive.HasValue)
                query = query.Where(r => r.IsActive == isActive.Value);

            var rooms = await query.ToListAsync();

            var result = rooms.Select(r => new RoomResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Code = r.Code,
                Building = r.Building,
                Capacity = r.Capacity,
                IsActive = r.IsActive
            });

            return Ok(result);
        }

        // GET ROOM BY ID
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomResponseDto>> GetById(int id)
        {
            var r = await _context.Rooms.FindAsync(id);

            if (r == null)
                return NotFound();

            var result = new RoomResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Code = r.Code,
                Building = r.Building,
                Capacity = r.Capacity,
                IsActive = r.IsActive
            };

            return Ok(result);
        }

        // CREATE ROOM (ADMIN ONLY)
        [HttpPost]
        public async Task<ActionResult<RoomResponseDto>> Create([FromBody] CreateRoomDto request, [FromQuery] int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.Role != UserRole.Admin)
                return Forbid("Hanya admin yang boleh menambah ruangan.");

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Name wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("Code wajib diisi.");

            var codeExists = await _context.Rooms
                .AnyAsync(r => r.Code == request.Code);

            if (codeExists)
                return BadRequest("Kode ruangan sudah digunakan.");

            var room = new Room
            {
                Name = request.Name,
                Code = request.Code,
                Building = request.Building,
                Capacity = request.Capacity,
                IsActive = true
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            var result = new RoomResponseDto
            {
                Id = room.Id,
                Name = room.Name,
                Code = room.Code,
                Building = room.Building,
                Capacity = room.Capacity,
                IsActive = room.IsActive
            };

            return Ok(result);
        }

        // UPDATE ROOM (ADMIN ONLY)
        [HttpPut("{id}")]
        public async Task<ActionResult<RoomResponseDto>> Update( int id, [FromBody] UpdateRoomDto request, [FromQuery] int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.Role != UserRole.Admin)
                return Forbid("Hanya admin yang boleh mengubah ruangan.");

            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Name wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("Code wajib diisi.");

            var codeExists = await _context.Rooms
                .AnyAsync(r => r.Code == request.Code && r.Id != id);

            if (codeExists)
                return BadRequest("Kode ruangan sudah digunakan.");

            room.Name = request.Name;
            room.Code = request.Code;
            room.Building = request.Building;
            room.Capacity = request.Capacity;
            room.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            var result = new RoomResponseDto
            {
                Id = room.Id,
                Name = room.Name,
                Code = room.Code,
                Building = room.Building,
                Capacity = room.Capacity,
                IsActive = room.IsActive
            };

            return Ok(result);
        }

        // DELETE ROOM (DEACTIVATE - ADMIN ONLY)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] int userId)

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
