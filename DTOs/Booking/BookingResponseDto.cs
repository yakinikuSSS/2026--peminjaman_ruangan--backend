public class BookingResponseDto
{
    public int Id { get; set; }

    public string BorrowerName { get; set; } = default!;
    public string BorrowerPhone { get; set; } = default!;

    public string Purpose { get; set; } = default!;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public BookingStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public RoomResponseDto Room { get; set; } = default!;
}
