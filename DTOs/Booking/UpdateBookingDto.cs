public class UpdateBookingDto
{
    public int RoomId { get; set; }
    public string Purpose { get; set; } = default!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
