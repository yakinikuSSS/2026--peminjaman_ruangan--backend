namespace PeminjamanRuangan.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public int TotalRooms { get; set; }
        public int TotalBookings { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }

        public List<RecentBookingDto> RecentBookings { get; set; } = new();
    }
}
