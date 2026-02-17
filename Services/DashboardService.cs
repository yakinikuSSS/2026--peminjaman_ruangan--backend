using Microsoft.EntityFrameworkCore;
using PeminjamanRuangan.DTOs.Dashboard;
using PeminjamanRuangan.Services.Interfaces;

namespace PeminjamanRuangan.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            await AutoUpdateStatuses();

            var totalRooms = await _context.Rooms
                .CountAsync(r => r.IsActive);

            var totalActiveBookings = await _context.RoomBookings
                .CountAsync(b =>
                    b.DeletedAt == null &&
                    (b.Status == BookingStatus.Pending ||
                    b.Status == BookingStatus.Approved)
                );

            var pending = await _context.RoomBookings
                .CountAsync(b =>
                    b.DeletedAt == null &&
                    b.Status == BookingStatus.Pending
                );

            var approved = await _context.RoomBookings
                .CountAsync(b =>
                    b.DeletedAt == null &&
                    b.Status == BookingStatus.Approved
                );

            var rejected = await _context.RoomBookings
                .CountAsync(b =>
                    b.DeletedAt == null &&
                    b.Status == BookingStatus.Rejected
                );

            var recentBookings = await _context.RoomBookings
                .Include(b => b.Room)
                .Where(b =>
                    b.DeletedAt == null &&
                    (b.Status == BookingStatus.Pending ||
                    b.Status == BookingStatus.Approved)
                )
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .Select(b => new RecentBookingDto
                {
                    Id = b.Id,
                    RoomName = b.Room.Name,
                    Date = b.StartTime,
                    Status = b.Status.ToString()
                })
                .ToListAsync();

            return new DashboardSummaryDto
            {
                TotalRooms = totalRooms,
                TotalBookings = totalActiveBookings,
                Pending = pending,
                Approved = approved,
                Rejected = rejected,
                RecentBookings = recentBookings
            };
        }

        private async Task AutoUpdateStatuses()
        {
            var now = DateTime.UtcNow;

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

            if (completedBookings.Any() || cancelledBookings.Any())
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}
