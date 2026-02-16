using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PeminjamanRuangan.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingController(ApplicationDbContext context) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;

        // HELPER - CHECK TIME CONFLICT
        private async Task<bool> HasTimeConflict(
            int roomId,
            DateTime startTime,
            DateTime endTime,
            int? excludeBookingId = null)
        {
            return await _context.RoomBookings
                .Where(b =>
                    b.RoomId == roomId &&
                    b.DeletedAt == null &&
                    b.Status == BookingStatus.Approved &&
                    b.StartTime < endTime &&
                    b.EndTime > startTime)
                .Where(b => !excludeBookingId.HasValue || b.Id != excludeBookingId.Value)
                .AnyAsync();
        }

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

            var isConflict = await HasTimeConflict(
                request.RoomId,
                request.StartTime,
                request.EndTime);

            if (isConflict)
                return BadRequest("Jadwal bentrok dengan booking Approved lain.");

            var booking = new RoomBooking
            {
                RoomId = request.RoomId,
                BorrowerName = request.BorrowerName,
                BorrowerPhone = request.BorrowerPhone,
                Purpose = request.Purpose,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now
            };

            _context.RoomBookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok(MapToDto(booking, room));
        }

        // GET ALL BOOKINGS
        [HttpGet]
        public async Task<IActionResult> GetAll(
            string? search,
            BookingStatus? status,
            string? sortBy,
            bool desc = false)
        {
            await AutoUpdateStatuses();
            var query = _context.RoomBookings
                .Include(b => b.Room)
                .Where(b => b.DeletedAt == null && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved))
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b =>
                    b.Purpose.Contains(search) ||
                    b.Room.Code.Contains(search) ||
                    b.BorrowerName.Contains(search));
            }

            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);

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

                    _ => query.OrderByDescending(b => b.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(b => b.CreatedAt);
            }

            var data = await query
                .Select(b => new BookingResponseDto
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
                })
                .ToListAsync();

            return Ok(data);
        }

        // GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _context.RoomBookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);

            if (booking == null)
                return NotFound();

            return Ok(MapToDto(booking, booking.Room));
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

            var isConflict = await HasTimeConflict(
                request.RoomId,
                request.StartTime,
                request.EndTime,
                id);

            if (isConflict)
                return BadRequest("Jadwal bentrok dengan booking Approved lain.");

            booking.RoomId = request.RoomId;
            booking.BorrowerName = request.BorrowerName;
            booking.BorrowerPhone = request.BorrowerPhone;
            booking.Purpose = request.Purpose;
            booking.StartTime = request.StartTime;
            booking.EndTime = request.EndTime;

            await _context.SaveChangesAsync();

            return Ok(MapToDto(booking, room));
        }

        // SOFT DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.RoomBookings.FindAsync(id);

            if (booking == null || booking.DeletedAt != null)
                return NotFound();

            booking.DeletedAt = DateTime.Now;
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

            // Validasi transisi status
            switch (booking.Status)
            {
                case BookingStatus.Pending:
                    if (request.Status != BookingStatus.Approved &&
                        request.Status != BookingStatus.Rejected)
                        return BadRequest("Pending hanya bisa menjadi Approved atau Rejected.");
                    break;

                case BookingStatus.Approved:
                    if (request.Status != BookingStatus.Completed &&
                        request.Status != BookingStatus.Cancelled)
                        return BadRequest("Approved hanya bisa menjadi Completed atau Cancelled.");
                    break;

                default:
                    return BadRequest("Status tidak dapat diubah lagi.");
            }

            // Jika mau approve, cek conflict
            if (request.Status == BookingStatus.Approved)
            {
                var hasConflict = await HasTimeConflict(
                    booking.RoomId,
                    booking.StartTime,
                    booking.EndTime,
                    booking.Id);

                if (hasConflict)
                    return BadRequest("Tidak dapat approve karena sudah ada booking Approved lain.");
            }

            booking.Status = request.Status;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                booking.Id,
                booking.Status
            });
        }

        // PRIVATE MAPPER
        private static BookingResponseDto MapToDto(RoomBooking booking, Room room)
        {
            return new BookingResponseDto
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
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(
            string? search,
            string? sortBy,
            bool desc = false)
        {
            await AutoUpdateStatuses();
            var query = _context.RoomBookings
                .Include(b => b.Room)
                .Where(b =>
                    b.DeletedAt == null &&
                    (b.Status == BookingStatus.Rejected ||
                    b.Status == BookingStatus.Completed ||
                    b.Status == BookingStatus.Cancelled))
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b =>
                    b.Purpose.Contains(search) ||
                    b.Room.Code.Contains(search) ||
                    b.BorrowerName.Contains(search));
            }

            query = sortBy?.ToLower() switch
            {
                "starttime" => desc
                    ? query.OrderByDescending(b => b.StartTime)
                    : query.OrderBy(b => b.StartTime),

                _ => query.OrderByDescending(b => b.CreatedAt)
            };

            var data = await query
                .Select(b => new BookingResponseDto
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
                })
                .ToListAsync();

            return Ok(data);
        }
        
        private async Task AutoUpdateStatuses()
        {
            var now = DateTime.Now;

            var completedBookings = await _context.RoomBookings
                .Where(b =>
                    b.Status == BookingStatus.Approved &&
                    b.EndTime <= now)
                .ToListAsync();

            foreach (var booking in completedBookings)
            {
                booking.Status = BookingStatus.Completed;
            }

            var cancelledBookings = await _context.RoomBookings
                .Where(b =>
                    b.Status == BookingStatus.Pending &&
                    b.EndTime <= now)
                .ToListAsync();

            foreach (var booking in cancelledBookings)
            {
                booking.Status = BookingStatus.Cancelled;
            }

            await _context.SaveChangesAsync();
        }
    }
}
