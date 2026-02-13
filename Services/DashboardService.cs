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
            var totalRooms = await _context.Rooms.CountAsync();
            var totalBookings = await _context.RoomBookings.CountAsync();

            var pending = await _context.RoomBookings
                .CountAsync(b => b.Status == BookingStatus.Pending);

            var approved = await _context.RoomBookings
                .CountAsync(b => b.Status == BookingStatus.Approved);

            var rejected = await _context.RoomBookings
                .CountAsync(b => b.Status == BookingStatus.Rejected);

            var recentBookings = await _context.RoomBookings
                .Include(b => b.Room)
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
                TotalBookings = totalBookings,
                Pending = pending,
                Approved = approved,
                Rejected = rejected,
                RecentBookings = recentBookings
            };
        }
    }
}
