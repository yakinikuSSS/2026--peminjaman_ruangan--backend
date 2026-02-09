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
        public async Task<IActionResult> Create(RoomBooking booking)
        {
            if (booking.StartTime >= booking.EndTime)
                return BadRequest("StartTime harus lebih kecil dari EndTime.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == booking.UserId && u.IsActive && u.DeletedAt == null);

            if (user == null)
                return BadRequest("User tidak valid.");

            if (user.Role != UserRole.Customer)
                return BadRequest("Hanya customer yang boleh membuat booking.");

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == booking.RoomId && r.IsActive);

            if (room == null)
                return BadRequest("Room tidak valid.");

            var isConflict = await _context.RoomBookings.AnyAsync(b =>
                b.RoomId == booking.RoomId &&
                b.DeletedAt == null &&
                booking.StartTime < b.EndTime &&
                booking.EndTime > b.StartTime);

            if (isConflict)
                return BadRequest("Jadwal bentrok dengan booking lain.");

            booking.Status = BookingStatus.Pending;

            _context.RoomBookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok(booking);
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
                .Include(b => b.User)
                .Where(b => b.DeletedAt == null)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b =>
                    b.Purpose.Contains(search) ||
                    b.Room.Code.Contains(search) ||
                    b.User.Name.Contains(search));
            }

            if (status.HasValue)
                query = query.Where(b => b.Status == status);

            // TAMBAHKAN DI SINI (SORTING)
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

            return Ok(await query.ToListAsync());
        }


        // GET BOOKING BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var booking = await _context.RoomBookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);

            if (booking == null)
                return NotFound();

            return Ok(booking);
        }

        // UPDATE BOOKING
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RoomBooking updated)
        {
            var booking = await _context.RoomBookings.FindAsync(id);
            if (booking == null || booking.DeletedAt != null)
                return NotFound();

            if (updated.StartTime >= updated.EndTime)
                return BadRequest("StartTime harus lebih kecil dari EndTime.");

            var isConflict = await _context.RoomBookings.AnyAsync(b =>
                b.Id != id &&
                b.RoomId == updated.RoomId &&
                b.DeletedAt == null &&
                updated.StartTime < b.EndTime &&
                updated.EndTime > b.StartTime);

            if (isConflict)
                return BadRequest("Jadwal bentrok.");

            booking.RoomId = updated.RoomId;
            booking.Purpose = updated.Purpose;
            booking.StartTime = updated.StartTime;
            booking.EndTime = updated.EndTime;

            await _context.SaveChangesAsync();

            return Ok(booking);
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

        // CHANGE STATUS (ADMIN ONLY)
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, BookingStatus status, int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

            if (user == null)
                return BadRequest("User tidak ditemukan.");

            if (user.Role != UserRole.Admin)
                return Forbid("Hanya admin yang boleh mengubah status.");

            var booking = await _context.RoomBookings.FindAsync(id);
            if (booking == null || booking.DeletedAt != null)
                return NotFound();

            booking.Status = status;

            await _context.SaveChangesAsync();

            return Ok(booking);
        }
    }
}
