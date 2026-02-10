using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PeminjamanRuangan.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        // CREATE BOOKING
        [HttpPost]
        public async Task<IActionResult> Create(CreateBookingDto request)
        {
            if (request.StartTime >= request.EndTime)
                return BadRequest("StartTime harus lebih kecil dari EndTime.");

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == request.RoomId && r.IsActive);

            if (room == null)
                return BadRequest("Room tidak valid.");

            var isConflict = await _context.RoomBookings.AnyAsync(b =>
                b.RoomId == request.RoomId &&
                b.DeletedAt == null &&
                request.StartTime < b.EndTime &&
                request.EndTime > b.StartTime);

            if (isConflict)
                return BadRequest("Jadwal bentrok dengan booking lain.");

            var booking = new RoomBooking
            {
                RoomId = request.RoomId,
                BorrowerName = request.BorrowerName,
                BorrowerPhone = request.BorrowerPhone,
                Purpose = request.Purpose,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.RoomBookings.Add(booking);
            await _context.SaveChangesAsync();

            var result = new BookingResponseDto
            {
                Id = booking.Id,
                BorrowerName = booking.BorrowerName,
                BorrowerPhone = booking.BorrowerPhone,
                Purpose = booking.Purpose,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt,
                Room = new RoomResponseDto
                {
                    Id = room.Id,
                    Name = room.Name,
                    Code = room.Code,
                    Building = room.Building,
                    Capacity = room.Capacity,
                    IsActive = room.IsActive
                }
            };

            return Ok(result);
        }

        // GET ALL BOOKINGS
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            BookingStatus? status,
            string? sortBy,
            bool desc = false)
        {
            var query = _context.RoomBookings
                .Include(b => b.Room)
                .Where(b => b.DeletedAt == null)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b =>
                    b.Purpose.Contains(search) ||
                    b.Room.Code.Contains(search) ||
                    b.BorrowerName.Contains(search));
            }

            if (status.HasValue)
                query = query.Where(b => b.Status == status);

            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortBy.ToLower() switch
                {
                    "starttime" => desc
                        ? query.OrderByDescending(b => b.StartTime)
                        : query.OrderBy(b => b.StartTime),

                    "createdat" => desc
                        ? query.OrderByDescending(b => b.CreatedAt)
                        : query.OrderBy(b => b.CreatedAt),

                    "status" => desc
                        ? query.OrderByDescending(b => b.Status)
                        : query.OrderBy(b => b.Status),

                    _ => query
                };
            }

            var data = await query.ToListAsync();

            var result = data.Select(b => new BookingResponseDto
            {
                Id = b.Id,
                BorrowerName = b.BorrowerName,
                BorrowerPhone = b.BorrowerPhone,
                Purpose = b.Purpose,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = b.Status,
                CreatedAt = b.CreatedAt,
                Room = new RoomResponseDto
                {
                    Id = b.Room.Id,
                    Name = b.Room.Name,
                    Code = b.Room.Code,
                    Building = b.Room.Building,
                    Capacity = b.Room.Capacity,
                    IsActive = b.Room.IsActive
                }
            });

            return Ok(result);
        }

        // GET BOOKING BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var b = await _context.RoomBookings
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

            if (b == null)
                return NotFound();

            var result = new BookingResponseDto
            {
                Id = b.Id,
                BorrowerName = b.BorrowerName,
                BorrowerPhone = b.BorrowerPhone,
                Purpose = b.Purpose,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                Status = b.Status,
                CreatedAt = b.CreatedAt,
                Room = new RoomResponseDto
                {
                    Id = b.Room.Id,
                    Name = b.Room.Name,
                    Code = b.Room.Code,
                    Building = b.Room.Building,
                    Capacity = b.Room.Capacity,
                    IsActive = b.Room.IsActive
                }
            };

            return Ok(result);
        }

        // UPDATE BOOKING
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateBookingDto request)
        {
            var booking = await _context.RoomBookings
                .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);

            if (booking == null)
                return NotFound();

            if (request.StartTime >= request.EndTime)
                return BadRequest("StartTime harus lebih kecil dari EndTime.");

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == request.RoomId && r.IsActive);

            if (room == null)
                return BadRequest("Room tidak valid.");

            var isConflict = await _context.RoomBookings.AnyAsync(b =>
                b.Id != id &&
                b.RoomId == request.RoomId &&
                b.DeletedAt == null &&
                request.StartTime < b.EndTime &&
                request.EndTime > b.StartTime);

            if (isConflict)
                return BadRequest("Jadwal bentrok.");

            booking.RoomId = request.RoomId;
            booking.BorrowerName = request.BorrowerName;
            booking.BorrowerPhone = request.BorrowerPhone;
            booking.Purpose = request.Purpose;
            booking.StartTime = request.StartTime;
            booking.EndTime = request.EndTime;

            await _context.SaveChangesAsync();

            var result = new BookingResponseDto
            {
                Id = booking.Id,
                BorrowerName = booking.BorrowerName,
                BorrowerPhone = booking.BorrowerPhone,
                Purpose = booking.Purpose,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt,
                Room = new RoomResponseDto
                {
                    Id = room.Id,
                    Name = room.Name,
                    Code = room.Code,
                    Building = room.Building,
                    Capacity = room.Capacity,
                    IsActive = room.IsActive
                }
            };
            return Ok(result);
        }

        // SOFT DELETE BOOKING
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.RoomBookings.FindAsync(id);
            if (booking == null || booking.DeletedAt != null)
                return NotFound();

            booking.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        // CHANGE STATUS
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, ChangeBookingStatusDto request)
        {
            var booking = await _context.RoomBookings
                .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);

            if (booking == null)
                return NotFound();

            booking.Status = request.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                booking.Id,
                booking.Status
            });
        }
    }
}
