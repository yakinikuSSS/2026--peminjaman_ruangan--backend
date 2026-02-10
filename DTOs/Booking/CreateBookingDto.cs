public class CreateBookingDto
{
    public int RoomId { get; set; }

    public string BorrowerName { get; set; } = default!;
    public string BorrowerPhone { get; set; } = default!;

    public string Purpose { get; set; } = default!;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
