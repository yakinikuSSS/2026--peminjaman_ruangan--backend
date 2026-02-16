using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeminjamanRuangan.DTOs.Room;
namespace PeminjamanRuangan.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    public class RoomController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoomController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET ALL ROOMS
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            string? building,
            bool? isActive,
            int pageNumber = 1,
            int pageSize = 10)
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

            var totalCount = await query.CountAsync();

            var rooms = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = rooms.Select(r => new RoomResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Code = r.Code,
                Building = r.Building,
                Capacity = r.Capacity,
                IsActive = r.IsActive
            });

            return Ok(new PaginatedResponseDto<RoomResponseDto>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Data = result
            });
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

        // CREATE ROOM
        [HttpPost]
        public async Task<ActionResult<RoomResponseDto>> Create(CreateRoomDto request)
        {
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

        // UPDATE ROOM
        [HttpPut("{id}")]
        public async Task<ActionResult<RoomResponseDto>> Update(int id, UpdateRoomDto request)
        {
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

        // DELETE ROOM (Deactivate)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
                return NotFound();

            room.IsActive = false;

            await _context.SaveChangesAsync();

            return Ok("Ruangan dihapus.");
        }

        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<AvailableRoomDto>>> GetAvailableRooms(
            string building,
            DateTime startTime,
            DateTime endTime)
        {
            if (startTime >= endTime)
                return BadRequest("Waktu tidak valid.");

            var rooms = _context.Rooms
                .Where(r => r.Building == building && r.IsActive);

            var availableRooms = await rooms
                .Where(r => !_context.RoomBookings
                    .Any(b =>
                        b.RoomId == r.Id &&
                        (b.Status == BookingStatus.Pending ||
                            b.Status == BookingStatus.Approved) &&
                        b.StartTime < endTime &&
                        b.EndTime > startTime))
                .Select(r => new AvailableRoomDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Building = r.Building,
                    Capacity = r.Capacity
                })
                .ToListAsync();

            return Ok(availableRooms);
        }

    }
}