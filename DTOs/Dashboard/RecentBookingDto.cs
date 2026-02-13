namespace PeminjamanRuangan.DTOs.Dashboard
{
    public class RecentBookingDto
    {
        public int Id { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
